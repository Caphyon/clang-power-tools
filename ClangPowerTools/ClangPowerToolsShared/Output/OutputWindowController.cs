using ClangPowerTools.Builder;
using ClangPowerTools.Commands;
using ClangPowerTools.Error;
using ClangPowerTools.Events;
using ClangPowerTools.Handlers;
using ClangPowerTools.Helpers;
using ClangPowerTools.Services;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace ClangPowerTools.Output
{
  public class OutputWindowController
  {
    #region Members

    private readonly OutputProcessor outputProcessor = new OutputProcessor();

    private IBuilder<OutputWindowModel> outputWindowBuilder;

    private OutputContentModel outputContent = new OutputContentModel();

    public event EventHandler<ErrorDetectedEventArgs> ErrorDetectedEvent;

    public event EventHandler<CloseDataConnectionEventArgs> CloseDataConnectionEvent;

    public event EventHandler<HasEncodingErrorEventArgs> HasEncodingErrorEvent;

    public event EventHandler<JsonFilePathArgs> JsonCompilationDbFilePathEvent;

    #endregion

    #region Properties

    private static Mutex mutex = new Mutex();
    public List<string> Buffer => outputContent.Buffer;

    public bool IsBufferEmpty => 0 == outputContent.Buffer.Count;

    public HashSet<TaskErrorModel> Errors => outputContent.Errors;

    public bool HasErrors => 0 != outputContent.Errors.Count;

    private IVsHierarchy Hierarchy { get; set; }
    private int machesNr = 0;

    private HashSet<string> paths;
    private List<string> tempPaths;

    #endregion

    public OutputWindowController()
    {
      paths = new HashSet<string>();
      tempPaths = new List<string>();
    }

    #region Methods

    #region Output window operations
    private Package package;
    public void Initialize(AsyncPackage aPackage, IVsOutputWindow aVsOutputWindow)
    {
      if (null == outputWindowBuilder)
        outputWindowBuilder = new OutputWindowBuilder(aPackage, aVsOutputWindow);

      outputWindowBuilder.Build();
      package = aPackage;
    }

    public void ClearPanel(object sender, ClearEventArgs e) => Clear();

    public void Clear()
    {
      outputContent = new OutputContentModel();
      var outputWindow = outputWindowBuilder.GetResult();

      UIUpdater.InvokeAsync(() =>
      {
        outputWindow.Pane.Clear();

      }).SafeFireAndForget();
    }

    public void Show()
    {
      if (!SettingsProvider.CompilerSettingsModel.ShowOutputWindow)
        return;

      var outputWindow = outputWindowBuilder.GetResult();

      UIUpdater.InvokeAsync(() =>
      {
        outputWindow.Pane.Activate();
        if (VsServiceProvider.TryGetService(typeof(DTE2), out object dte))
        {
          (dte as DTE2).ExecuteCommand("View.Output", string.Empty);
        }
        VsWindowController.Activate(VsWindowController.PreviousWindow);
      }).SafeFireAndForget();
    }

    public void Write(string aMessage)
    {
      if (string.IsNullOrWhiteSpace(aMessage))
        return;

      mutex.WaitOne();
      var outputWindow = outputWindowBuilder.GetResult();
      outputWindow.Pane.OutputStringThreadSafe(aMessage + "\n");
      mutex.ReleaseMutex();
    }

    public void Write(object sender, ClangCommandMessageEventArgs e)
    {
      if (e.ClearFlag)
      {
        Clear();
      }
      Show();
      Write(e.Message);
    }

    protected virtual void OnFileHierarchyChanged(object sender, VsHierarchyDetectedEventArgs e)
    {
      if (null == e.Hierarchy)
        return;
      Hierarchy = e.Hierarchy;
    }

    #endregion

    #region Data Handlers

    public void GetFilesFromOutput(string output)
    {
      if (output == null)
        return;

      Regex regex = new Regex(ErrorParserConstants.kMatchTidyFileRegex);
      Match match = regex.Match(output);

      while (match.Success)
      {
        paths.Add(match.Groups[1].Value.Trim());
        match = match.NextMatch();
      }
    }

    public void OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
      var id = CommandControllerInstance.CommandController.GetCurrentCommandId();
      if (null == e.Data)
        return;

      if (outputContent.MissingLLVM)
        return;

      if (id == CommandIds.kTidyId || id == CommandIds.kTidyToolbarId
        || id == CommandIds.kTidyToolWindowId || id == CommandIds.kTidyFixId
        || id == CommandIds.kTidyFixToolbarId)
      {
        GetFilesFromOutput(e.Data.ToString());
      }

      mutex.WaitOne();
      var result = outputProcessor.ProcessData(e.Data, Hierarchy, outputContent);
      mutex.ReleaseMutex();
      if (VSConstants.S_FALSE == result)
      {
        if (outputContent.MissingLLVM)
        {
          Write(new object(), new ClangCommandMessageEventArgs(ErrorParserConstants.kMissingLlvmMessage, false));
        }
        return;
      }

      if (!string.IsNullOrWhiteSpace(outputContent.JsonFilePath))
        JsonCompilationDbFilePathEvent?.Invoke(this, new JsonFilePathArgs(outputContent.JsonFilePath));

      if ((id == CommandIds.kClangFind || id == CommandIds.kClangFindRun) &&
        !SettingsProvider.CompilerSettingsModel.VerboseMode)
        return;

      Write(outputContent.Text);
    }

    public void OutputDataErrorReceived(object sender, DataReceivedEventArgs e)
    {
      if (null == e.Data)
        return;

      if (outputContent.MissingLLVM)
        return;
      mutex.WaitOne();
      var result = outputProcessor.ProcessData(e.Data, Hierarchy, outputContent);
      mutex.ReleaseMutex();
      if (VSConstants.S_FALSE == result)
        return;

      if (!string.IsNullOrWhiteSpace(outputContent.JsonFilePath))
        JsonCompilationDbFilePathEvent?.Invoke(this, new JsonFilePathArgs(outputContent.JsonFilePath));

      var id = CommandControllerInstance.CommandController.GetCurrentCommandId();
      if ((id == CommandIds.kClangFind || id == CommandIds.kClangFindRun) &&
        !SettingsProvider.CompilerSettingsModel.VerboseMode)
        return;

      Write(outputContent.Text);
    }

    public void ClosedDataConnection(object sender, EventArgs e)
    {
      mutex.WaitOne();
      string outputResult = String.Empty;
      var id = CommandControllerInstance.CommandController.GetCurrentCommandId();

      tempPaths.Clear();
      if (Buffer.Count != 0 && outputContent.MissingLLVM == false)
      {
        outputResult = String.Join("\n", Buffer);
        if (id == CommandIds.kClangFindRun)
        {
          Regex regex = new Regex(ErrorParserConstants.kNumberMatchesRegex);
          var matchResult = regex.Match(outputResult);
          if (matchResult != null && matchResult.Groups[0] != null
            && matchResult.Groups[0].Value != null
            && matchResult.Groups[0].Value.ToString() != string.Empty)
          {
            machesNr += Int32.Parse(matchResult.Groups[1].Value);
          }
        }
      }
      CloseDataConnectionEvent?.Invoke(this, new CloseDataConnectionEventArgs());

      OnErrorDetected(this, e);

      //open tidy tool window and pass paths
      var tidySettings = SettingsProvider.TidySettingsModel;
      if (id == CommandIds.kTidyToolWindowId || (id == CommandIds.kTidyFixId && !tidySettings.ApplyTidyFix))
      {
        Write(outputResult);
        foreach (var path in paths)
        {
          tempPaths.Add(path);
        }
        CommandControllerInstance.CommandController.LaunchCommandAsync(CommandIds.kTidyToolWindowFilesId, CommandUILocation.ContextMenu, tempPaths);
        paths.Clear();
      }
      mutex.ReleaseMutex();
    }

    public void OnFileHierarchyDetected(object sender, VsHierarchyDetectedEventArgs e)
    {
      Hierarchy = e.Hierarchy;
    }

    #endregion

    public void OnErrorDetected(object sender, EventArgs e)
    {
      mutex.WaitOne();
      if (Errors.Count > 0)
      {
        TaskErrorViewModel.Errors = Errors.ToList();

        TaskErrorViewModel.FileErrorsPair = new Dictionary<string, List<TaskErrorModel>>();
        foreach (var error in TaskErrorViewModel.Errors)
        {
          if (TaskErrorViewModel.FileErrorsPair.ContainsKey(error.Document))
          {
            TaskErrorViewModel.FileErrorsPair[error.Document].Add(error);
          }
          else
          {
            TaskErrorViewModel.FileErrorsPair.Add(error.Document, new List<TaskErrorModel>() { error });
          }
        }

        ErrorDetectedEvent?.Invoke(this, new ErrorDetectedEventArgs(Errors));
      }
      mutex.ReleaseMutex();
    }

    public void WriteMatchesNr()
    {
      Write($"🔎 We found {machesNr.ToString()} matches");
    }

    public void ResetMatchesNr()
    {
      machesNr = 0;
    }

    public void OnEncodingErrorDetected(object sender, EventArgs e)
    {
      HasEncodingErrorEvent?.Invoke(this, new HasEncodingErrorEventArgs(outputContent));
    }

    #endregion

  }
}
