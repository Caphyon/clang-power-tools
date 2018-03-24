using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ClangPowerTools
{
  public class OutputManager
  {
    #region Members

    private DTE2 mDte = null;

    private int kBufferSize = 5;
    private List<string> mMessagesBuffer = new List<string>();

    private ErrorParser mErrorParser = new ErrorParser();

    private bool mMissingLlvm = false;
    private HashSet<TaskError> mErrors = new HashSet<TaskError>();

    private List<string> mPCHPaths = new List<string>();

    #endregion

    #region Properties

    public bool MissingLlvm => mMissingLlvm;

    public List<string> Buffer => mMessagesBuffer;

    public bool EmptyBuffer => mMessagesBuffer.Count == 0;

    public HashSet<TaskError> Errors => mErrors;

    public bool HasErrors => 0 != mErrors.Count;

    public List<string> PCHPaths => mPCHPaths;

    public IVsHierarchy Hierarchy { get; set; }

    #endregion

    #region Constructor

    public OutputManager(DTE2 aDte)
    {
      mDte = aDte;
    }

    #endregion

    #region Public Methods

    public void Clear()
    {
      using (OutputWindow outputWindow = new OutputWindow(mDte))
      {
        DispatcherHandler.BeginInvoke(() =>
        {
          outputWindow.Clear();
        });
      }
    }

    public void Show()
    {
      using (OutputWindow outputWindow = new OutputWindow(mDte))
      {
        DispatcherHandler.BeginInvoke(() =>
        {
          outputWindow.Show(mDte);
        });
      }
    }

    public void AddMessage(string aMessage)
    {
      if (String.IsNullOrWhiteSpace(aMessage))
        return;

      using (OutputWindow outputWindow = new OutputWindow(mDte))
      {
        DispatcherHandler.BeginInvoke(() =>
        {
          outputWindow.Write(aMessage);
        });
      }
    }

    private void ProcessOutput(string aMessage)
    {
      try
      {
        if (mErrorParser.LlvmIsMissing(aMessage))
        {
          mMissingLlvm = true;
        }
        else if (!mMissingLlvm)
        {
          string text = String.Join("\n", mMessagesBuffer) + "\n";

          // Find error messages from powershell script output
          // replace them with an error message format that VS output window knows to interpret
          if (mErrorParser.FindErrors(text, out TaskError aError))
          {
            aError.HierarchyItem = Hierarchy;
            List<TaskError> errors = new List<TaskError>();
            errors.Add(aError);

            StringBuilder output = new StringBuilder(
              GetOutput(ref text, aError.FullMessage ));

            while (mErrorParser.FindErrors(text, out aError))
            {
              aError.HierarchyItem = Hierarchy;
              errors.Add(aError);
              output.Append(GetOutput(ref text, aError.FullMessage));
            }

            AddMessage(output.ToString());
            output.Clear();

            if (0 != mMessagesBuffer.Count)
              mMessagesBuffer.Clear();

            SaveErrorsMessages(errors);
          }
          else if (kBufferSize <= mMessagesBuffer.Count)
          {
            AddMessage(mMessagesBuffer[0]);
            mMessagesBuffer.RemoveAt(0);
          }
        }
      }
      catch (Exception)
      {

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

    #region Private Methods

    private void SaveErrorsMessages(List<TaskError> aErrorCollection)
    {
      if (0 == aErrorCollection.Count)
        return;

      foreach (var newError in aErrorCollection)
      {
        if (null == newError)
          continue;
        mErrors.Add(newError);
      }
    }

    private string GetOutput(ref string aText, string aSearchedSubstring)
    {
      aText = mErrorParser.Format(aText, aSearchedSubstring);

      GetBeforeAndAfterSubstrings(aText, aSearchedSubstring,
        out string substringBefore, out string substringAfter);

      aText = substringAfter;
      return substringBefore + aSearchedSubstring;
    }

    private void GetBeforeAndAfterSubstrings(string aText, string aSearchedSubstring, out string aTextBefore, out string aTextAfter)
    {
      aTextBefore = StringExtension.SubstringBefore(aText, aSearchedSubstring);
      aTextAfter = StringExtension.SubstringAfter(aText, aSearchedSubstring);
    }

    #endregion

  }
}
