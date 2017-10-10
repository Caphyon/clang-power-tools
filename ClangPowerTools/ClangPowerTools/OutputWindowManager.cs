using EnvDTE80;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Interop;
using System.Windows.Threading;

namespace ClangPowerTools
{
  public class OutputWindowManager
  {
    #region Members

    private DTE2 mDte = null;
    private Dispatcher mDispatcher;

    private const string kCompileErrorsRegex = @"(.\:\\[ \w+\\\/.]*[h|cpp])(\r\n|\r|\n| |:)*(\d+)(\r\n|\r|\n| |:)*(\d+)(\r\n|\r|\n| |:)*error(\r\n|\r|\n| |:)*(.*)";
    private int kBufferSize = 5;
    private List<string> mMessagesBuffer = new List<string>();

    private ErrorCreator errorCreator = new ErrorCreator();
    private bool mMissingLlvm = false;
    private List<ScriptError> mErrors = new List<ScriptError>();

    #endregion

    #region Properties

    public bool MissingLlvm => mMissingLlvm;

    public List<string> Buffer => mMessagesBuffer;

    public bool EmptyBuffer => mMessagesBuffer.Count == 0;

    public List<ScriptError> Errors => mErrors;

    #endregion

    #region Constructor

    public OutputWindowManager(DTE2 aDte)
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

    public void AddMessage(string aMessage)
    {
      if (String.IsNullOrWhiteSpace(aMessage))
        return;

      using (OutputWindow outputWindow = new OutputWindow(mDte))
      {
        outputWindow.Show(mDte);
        mDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
        {
          outputWindow.Write(aMessage);
        }));
      }
    }

    public void ProcessOutput(string aMessage)
    {
      if (errorCreator.LlvmIsMissing(aMessage))
      {
        mMissingLlvm = true;
      }
      else if (!mMissingLlvm)
      {
        string messages = String.Join("\n", mMessagesBuffer);
        if (errorCreator.FindErrors(messages, out ScriptError aError))
        {
          messages = errorCreator.Format(messages, aError.FullMessage);
          AddMessage(messages);
          mMessagesBuffer.Clear();
          if( null != aError )
            mErrors.Add(aError);
        }
        else if (kBufferSize <= mMessagesBuffer.Count)
        {
          //foreach (string message in mMessagesBuffer)
          //  AddMessage(message);

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
      //mOutputManager.AddMessage(e.Data);
      //mOutputMessages.AppendLine(e.Data);
    }


    #endregion

  }
}
