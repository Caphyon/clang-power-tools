using ClangPowerTools.Builder;
using ClangPowerTools.Error;
using ClangPowerTools.Events;
using ClangPowerTools.Handlers;
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

    private OutputProcessor mOutputProcessor = new OutputProcessor();

    private IBuilder<OutputWindowModel> mOutputWindowBuilder;

    private OutputContentModel mOutputContent = new OutputContentModel();

    public event EventHandler<ErrorDetectedEventArgs> ErrorDetectedEvent;

    public event EventHandler<CloseDataConnectionEventArgs> CloseDataConnectionEvent;

    public event EventHandler<HasEncodingErrorEventArgs> HasEncodingErrorEvent;

    #endregion

    #region Properties


    public List<string> Buffer => mOutputContent.Buffer;

    public bool IsBufferEmpty => 0 == mOutputContent.Buffer.Count;

    public HashSet<TaskErrorModel> Errors => mOutputContent.Errors;

    public bool HasErrors => 0 != mOutputContent.Errors.Count;

    private IVsHierarchy Hierarchy { get; set; }


    #endregion

    #region Methods

    #region Output window operations

    public void Initialize(AsyncPackage aPackage, IVsOutputWindow aVsOutputWindow)
    {
      if (null == mOutputWindowBuilder)
        mOutputWindowBuilder = new OutputWindowBuilder(aPackage, aVsOutputWindow);

      mOutputWindowBuilder.Build();
    }

    public void ClearPanel(object sender, ClearEventArgs e) => Clear();

    public void Clear()
    {
      mOutputContent = new OutputContentModel();
      var outputWindow = mOutputWindowBuilder.GetResult();

      UIUpdater.InvokeAsync(() =>
      {
        outputWindow.Pane.Clear();

      }).SafeFireAndForget();
    }

    public void Show()
    {
      var outputWindow = mOutputWindowBuilder.GetResult();

      UIUpdater.InvokeAsync(() =>
      {
        outputWindow.Pane.Activate();
        if (VsServiceProvider.TryGetService(typeof(DTE), out object dte))
        {
          (dte as DTE2).ExecuteCommand("View.Output", string.Empty);
        }
      }).SafeFireAndForget();
    }

    public void Write(string aMessage)
    {
      if (string.IsNullOrWhiteSpace(aMessage))
        return;

      var outputWindow = mOutputWindowBuilder.GetResult();
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

      if (mOutputContent.MissingLLVM)
        return;

      if (VSConstants.S_FALSE == mOutputProcessor.ProcessData(e.Data, Hierarchy, mOutputContent))
      {
        if (mOutputContent.MissingLLVM)
        {
          Write(new object(), new ClangCommandMessageEventArgs(ErrorParserConstants.kMissingLlvmMessage, false));
        }
        return;
      }

      Write(mOutputContent.Text);
    }

    public void OutputDataErrorReceived(object sender, DataReceivedEventArgs e)
    {
      if (null == e.Data)
        return;

      if (mOutputContent.MissingLLVM)
        return;

      if (VSConstants.S_FALSE == mOutputProcessor.ProcessData(e.Data, Hierarchy, mOutputContent))
        return;

      Write(mOutputContent.Text);
    }

    public void ClosedDataConnection(object sender, EventArgs e)
    {
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
      HasEncodingErrorEvent?.Invoke(this, new HasEncodingErrorEventArgs(mOutputContent));
    }

    #endregion

  }
}
