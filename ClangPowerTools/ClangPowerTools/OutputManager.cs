using EnvDTE80;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Interop;
using System.Windows.Threading;

namespace ClangPowerTools
{
  public class OutputManager
  {
    #region Members

    private DTE2 mDte = null;
    private Dispatcher mDispatcher;

    private int kBufferSize = 5;
    private List<string> mMessagesBuffer = new List<string>();

    private ErrorParser mParser = new ErrorParser();
    private bool mMissingLlvm = false;
    private List<TaskError> mErrors = new List<TaskError>();

    #endregion

    #region Properties

    public bool MissingLlvm => mMissingLlvm;
    public List<string> Buffer => mMessagesBuffer;
    public bool EmptyBuffer => mMessagesBuffer.Count == 0;
    public List<TaskError> Errors => mErrors;
    public bool HasErrors => 0 != mErrors.Count;
    #endregion

    #region Constructor

    public OutputManager(DTE2 aDte)
    {
      mDte = aDte;
      mDispatcher = HwndSource.FromHwnd((IntPtr)mDte.MainWindow.HWnd).RootVisual.Dispatcher;
    }

    #endregion

    #region Public Methods

    public void Clear()
    {
      using (OutputWindow outputWindow = new OutputWindow(mDte))
      {
        mDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
        {
          outputWindow.Clear();
        }));
      }
    }

    public void Show()
    {
      using (OutputWindow outputWindow = new OutputWindow(mDte))
        outputWindow.Show(mDte);
    }

    public void AddMessage(string aMessage)
    {
      if (String.IsNullOrWhiteSpace(aMessage))
        return;

      using (OutputWindow outputWindow = new OutputWindow(mDte))
      {
        mDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
        {
          outputWindow.Write(aMessage);
        }));
      }
    }

    public void ProcessOutput(string aMessage)
    {
      if (mParser.LlvmIsMissing(aMessage))
      {
        mMissingLlvm = true;
      }
      else if (!mMissingLlvm)
      {
        string messages = String.Join("\n", mMessagesBuffer);
        if (mParser.FindErrors(messages, out TaskError aError))
        {
          messages = mParser.Format(messages, aError.FullMessage);
          AddMessage(messages);
          mMessagesBuffer.Clear();
          if( null != aError )
            mErrors.Add(aError);
        }
        else if (kBufferSize <= mMessagesBuffer.Count)
        {
          AddMessage(mMessagesBuffer[0]);
          mMessagesBuffer.RemoveAt(0);
        }
      }
    }

    public void OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
      if (null == e.Data)
        return;
      mMessagesBuffer.Add(e.Data);
      ProcessOutput(e.Data);
    }

    public void OutputDataErrorReceived(object sender, DataReceivedEventArgs e)
    {
      if (null == e.Data)
        return;
      mMessagesBuffer.Add(e.Data);
      ProcessOutput(e.Data);
    }

    #endregion

  }
}
