using ClangPowerTools.Builder;
using ClangPowerTools.Handlers;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ClangPowerTools.Output
{
  public class OutputWindowController
  {
    #region Private Members


    private OutputProcessor mOutputProcessor = new OutputProcessor();

    private IAsyncBuilder<OutputWindowModel> mOutputWindowBuilder;

    private OutputContentModel mOutputContent = new OutputContentModel();

    private DTE2 mDte = null;


    #endregion


    #region Properties


    public bool MissingLlvm => mOutputContent.MissingLLVM;

    public List<string> Buffer => mOutputContent.Buffer;

    public bool IsBufferEmpty => 0 == mOutputContent.Buffer.Count;

    public HashSet<TaskErrorModel> Errors => mOutputContent.Errors;

    public bool HasErrors => 0 != mOutputContent.Errors.Count;

    public IVsHierarchy Hierarchy { get; set; }


    #endregion


    #region Methods


    #region Output window operations


    public async Task<object> InitializeAsync(AsyncPackage aPackage, DTE2 aDte)
    {
      mDte = aDte;
      if (null == mOutputWindowBuilder)
        mOutputWindowBuilder = new OutputWindowBuilder(aPackage);

      return await mOutputWindowBuilder.AsyncBuild();
    }


    public void Clear()
    {
      mOutputContent = new OutputContentModel();
      var outputWindow = mOutputWindowBuilder.GetAsyncResult();
      UIUpdater.Invoke(() =>
      {
        outputWindow.Pane.Clear();
      });
    }

    public void Show()
    {
      var outputWindow = mOutputWindowBuilder.GetAsyncResult();
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

      var outputWindow = mOutputWindowBuilder.GetAsyncResult();
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


    public void ClosedDataConnection(object sender, EventArgs e)
    {
      if (0 == Buffer.Count)
        return;

      Write(String.Join("\n", Buffer));
    }


    #endregion


    #endregion

  }
}
