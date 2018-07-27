using ClangPowerTools.Error;
using ClangPowerTools.Handlers;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ClangPowerTools.Output
{
  public class OutputWindowController
  {
    #region Private Members


    private OutputProcessor mOutputProcessor = new OutputProcessor();

    private IBuilder<OutputWindowModel> mOutputWindowBuilder;

    private OutputContentModel mOutputContent = new OutputContentModel();

    private DTE2 mDte = null;


    #endregion


    #region Properties


    public bool MissingLlvm => mOutputContent.MissingLLVM;

    public bool IsBufferEmpty => 0 == mOutputContent.Buffer.Count;
    public List<string> Buffer => mOutputContent.Buffer;

    public HashSet<TaskErrorModel> Errors => mOutputContent.Errors;
    public bool HasErrors => 0 != mOutputContent.Errors.Count;

    public IVsHierarchy Hierarchy { get; set; }


    #endregion


    #region Constuctor


    public OutputWindowController(AsyncPackage aPackage, DTE2 aDte)
    {
      mDte = aDte;
      if (null == mOutputWindowBuilder)
        mOutputWindowBuilder = new OutputWindowBuilder(aPackage);

      mOutputWindowBuilder.Build();
    }

    #endregion


    #region Methods


    #region Output window operations


    public void Clear()
    {
      mOutputContent = new OutputContentModel();
      var outputWindow = mOutputWindowBuilder.GetResult();
      UIUpdater.Invoke(() =>
      {
        outputWindow.Pane.Clear();
      });
    }

    public void Show()
    {
      var outputWindow = mOutputWindowBuilder.GetResult();
      UIUpdater.Invoke(() =>
      {
        outputWindow.Pane.Activate();
        mDte.ExecuteCommand("View.Output", string.Empty);
      });
    }

    public void Write(string aMessage)
    {
      if (String.IsNullOrWhiteSpace(aMessage))
        return;

      var outputWindow = mOutputWindowBuilder.GetResult();
      UIUpdater.Invoke(() =>
      {
        outputWindow.Pane.OutputStringThreadSafe(aMessage + "\n");
      });
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
        return;

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


    #endregion


    #endregion

  }
}
