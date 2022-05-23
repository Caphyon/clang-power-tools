using ClangPowerTools.Builder;
using ClangPowerTools.Commands;
using ClangPowerTools.Error;
using ClangPowerTools.Events;
using ClangPowerTools.Handlers;
using ClangPowerTools.Helpers;
using ClangPowerTools.Services;
using ClangPowerToolsShared.MVVM.Views.ToolWindows;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

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


    public List<string> Buffer => outputContent.Buffer;

    public bool IsBufferEmpty => 0 == outputContent.Buffer.Count;

    public HashSet<TaskErrorModel> Errors => outputContent.Errors;

    public bool HasErrors => 0 != outputContent.Errors.Count;

    private IVsHierarchy Hierarchy { get; set; }

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

      var outputWindow = outputWindowBuilder.GetResult();
      outputWindow.Pane.OutputStringThreadSafe(aMessage + "\n");
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

      Regex regex = new Regex(@"([A-Z]:\\.+?\.(cpp|cu|cc|cp|tlh|c|cxx|tli|h|hh|hpp|hxx))(\W|$)");
      Match match = regex.Match(output);

      while (match.Success)
      {
        paths.Add(match.Groups[1].Value.Trim());
        match = match.NextMatch();
      }
    }

    public void OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
      if (null == e.Data)
        return;

      if (outputContent.MissingLLVM)
        return;

      GetFilesFromOutput(e.Data.ToString());
      if (VSConstants.S_FALSE == outputProcessor.ProcessData(e.Data, Hierarchy, outputContent))
      {
        if (outputContent.MissingLLVM)
        {
          Write(new object(), new ClangCommandMessageEventArgs(ErrorParserConstants.kMissingLlvmMessage, false));
        }
        return;
      }

      if (!string.IsNullOrWhiteSpace(outputContent.JsonFilePath))
        JsonCompilationDbFilePathEvent?.Invoke(this, new JsonFilePathArgs(outputContent.JsonFilePath));
      Write(outputContent.Text);
    }

    public void OutputDataErrorReceived(object sender, DataReceivedEventArgs e)
    {
      if (null == e.Data)
        return;

      if (outputContent.MissingLLVM)
        return;

      if (VSConstants.S_FALSE == outputProcessor.ProcessData(e.Data, Hierarchy, outputContent))
        return;

      if (!string.IsNullOrWhiteSpace(outputContent.JsonFilePath))
        JsonCompilationDbFilePathEvent?.Invoke(this, new JsonFilePathArgs(outputContent.JsonFilePath));

      Write(outputContent.Text);
    }

    public void ClosedDataConnection(object sender, EventArgs e)
    {
      tempPaths.Clear();
      if (Buffer.Count != 0 && outputContent.MissingLLVM == false)
        Write(String.Join("\n", Buffer));

      CloseDataConnectionEvent?.Invoke(this, new CloseDataConnectionEventArgs());
      OnErrorDetected(this, e);

      //open tidy tool window and pass paths
      var id = CommandControllerInstance.CommandController.GetCurrentCommandId();
      var tidySettings = SettingsProvider.TidySettingsModel;
      if (id == CommandIds.kTidyToolWindowId || (id == CommandIds.kTidyFixId && !tidySettings.ApplyTidyFix))
      {
        foreach (var path in paths)
        {
          tempPaths.Add(path);
        }
        CommandControllerInstance.CommandController.LaunchCommandAsync(CommandIds.kTidyToolWindowFilesId, CommandUILocation.ContextMenu, tempPaths);
        paths.Clear();
      }
    }

    public void OnFileHierarchyDetected(object sender, VsHierarchyDetectedEventArgs e)
    {
      Hierarchy = e.Hierarchy;
    }

    #endregion

    public void OnErrorDetected(object sender, EventArgs e)
    {
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
    }

    public void OnEncodingErrorDetected(object sender, EventArgs e)
    {
      HasEncodingErrorEvent?.Invoke(this, new HasEncodingErrorEventArgs(outputContent));
    }

    #endregion

  }
}
