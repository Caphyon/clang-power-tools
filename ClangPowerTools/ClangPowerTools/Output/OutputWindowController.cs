using ClangPowerTools.Error;
using ClangPowerTools.Handlers;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace ClangPowerTools.Output
{
  public class OutputWindowController
  {
    #region Private Members

    private OutputProcessor mOutputProcessor = new OutputProcessor();

    private static IBuilder<OutputWindowModel> mOutputWindowBuilder;


    private DTE2 mDte = null;

    private List<string> mMessagesBuffer = new List<string>();

    private ErrorDetector mErrorParser = new ErrorDetector();

    private bool mMissingLlvm = false;
    private HashSet<TaskErrorModel> mErrors = new HashSet<TaskErrorModel>();

    private List<string> mPCHPaths = new List<string>();


    #endregion


    #region Properties


    public bool MissingLlvm => mMissingLlvm;

    public List<string> Buffer => mMessagesBuffer;

    public bool EmptyBuffer => mMessagesBuffer.Count == 0;

    public HashSet<TaskErrorModel> Errors => mErrors;

    public bool HasErrors => 0 != mErrors.Count;

    public List<string> PCHPaths => mPCHPaths;

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
      UIUpdater.Invoke(() =>
      {
        var outputWindow = mOutputWindowBuilder.GetResult();
        outputWindow.Pane.Clear();
      });
    }

    public void Show()
    {
      UIUpdater.Invoke(() =>
      {
        var outputWindow = mOutputWindowBuilder.GetResult();
        outputWindow.Pane.Activate();
        mDte.ExecuteCommand("View.Output", string.Empty);
      });
    }

    public void Write(string aMessage)
    {
      UIUpdater.Invoke(() =>
      {
        if (String.IsNullOrWhiteSpace(aMessage))
          return;

        var outputWindow = mOutputWindowBuilder.GetResult();
        outputWindow.Pane.OutputStringThreadSafe(aMessage + "\n");
      });
    }


    #endregion


    #region Data Handlers


    public void OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
      if (null == e.Data)
        return;

      if (!mOutputProcessor.MissingLLVM)
      {
        mOutputProcessor.ProcessData(e.Data, Hierarchy, out string message);
        Write(message);
      }
    }


    public void OutputDataErrorReceived(object sender, DataReceivedEventArgs e)
    {
      if (null == e.Data)
        return;

      if (!mOutputProcessor.MissingLLVM)
      {
        mOutputProcessor.ProcessData(e.Data, Hierarchy, out string message);
        Write(message);
      }
    }


    #endregion


    #endregion


  }
}
