using ClangPowerTools.Error;
using ClangPowerTools.Handlers;
using EnvDTE80;
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


    private static IBuilder<OutputWindowModel> mOutputWindowBuilder;


    private DTE2 mDte = null;

    private int kBufferSize = 5;
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


    public OutputWindowController(DTE2 aDte)
    {
      mDte = aDte;
      if (null == mOutputWindowBuilder)
        mOutputWindowBuilder = new OutputWindowBuilder(mDte);

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
        outputWindow.Pane.OutputStringThreadSafe(aMessage);
      });
    }


    #endregion


    #region Process Data


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
          if (mErrorParser.Detect(text, out Match aMarchResult))
          {
            IBuilder<TaskErrorModel> errorBuilder = new TaskErrorModelBuilder(aMarchResult);
            errorBuilder.Build();
            TaskErrorModel error = errorBuilder.GetResult();

            error.HierarchyItem = Hierarchy;
            List<TaskErrorModel> errors = new List<TaskErrorModel>();
            errors.Add(error);

            StringBuilder output = new StringBuilder(
              GetOutput(ref text, error.FullMessage));

            while (mErrorParser.Detect(text, out aMarchResult))
            {
              errorBuilder = new TaskErrorModelBuilder(aMarchResult);
              errorBuilder.Build();
              error = errorBuilder.GetResult();

              error.HierarchyItem = Hierarchy;
              errors.Add(error);
              output.Append(GetOutput(ref text, error.FullMessage));
            }

            Write(output.ToString());
            output.Clear();

            if (0 != mMessagesBuffer.Count)
              mMessagesBuffer.Clear();

            SaveErrorsMessages(errors);
          }
          else if (kBufferSize <= mMessagesBuffer.Count)
          {
            Write(mMessagesBuffer[0]);
            mMessagesBuffer.RemoveAt(0);
          }
        }
      }
      catch (Exception)
      {
      }
    }


    #endregion


    #region Data Handlers


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


    #region Helper Methods


    private void SaveErrorsMessages(List<TaskErrorModel> aErrorCollection)
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
      var errorFormatter = new ErrorFormatter();
      aText = errorFormatter.Format(aText, aSearchedSubstring);

      var substringBefore = aText.SubstringBefore(aSearchedSubstring);
      var substringAfter = aText.SubstringAfter(aSearchedSubstring);

      aText = substringAfter;
      return substringBefore + aSearchedSubstring;
    }


    #endregion


    #endregion


  }
}
