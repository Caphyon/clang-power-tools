﻿using ClangPowerTools.Builder;
using ClangPowerTools.Error;
using ClangPowerTools.Events;
using ClangPowerTools.Handlers;
using ClangPowerTools.Helpers;
using ClangPowerTools.Services;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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


    #endregion

    #region Methods

    #region Output window operations

    public void Initialize(AsyncPackage aPackage, IVsOutputWindow aVsOutputWindow)
    {
      if (null == outputWindowBuilder)
        outputWindowBuilder = new OutputWindowBuilder(aPackage, aVsOutputWindow);

      outputWindowBuilder.Build();
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

    public void OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
      if (null == e.Data)
        return;

      if (outputContent.MissingLLVM)
        return;

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
      if (Buffer.Count != 0 && outputContent.MissingLLVM == false)
        Write(String.Join("\n", Buffer));

      CloseDataConnectionEvent?.Invoke(this, new CloseDataConnectionEventArgs());
      OnErrorDetected(this, e);
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
