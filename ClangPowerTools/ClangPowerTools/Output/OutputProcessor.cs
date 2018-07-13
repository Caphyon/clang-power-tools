using ClangPowerTools.Error;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ClangPowerTools.Output
{
  public class OutputProcessor
  {
    #region Members 

    private ErrorDetector mErrorParser = new ErrorDetector();

    private readonly int kBufferSize = 5;


    #endregion

    #region Properties

    public List<string> Buffer { get; private set; } = new List<string>();

    public bool MissingLLVM { get; private set; } = false;

    private HashSet<TaskErrorModel> mErrors = new HashSet<TaskErrorModel>();


    #endregion


    #region Public methods


    public void ProcessData(string aMessage, IVsHierarchy aHierarchy, out string aOutputMessage)
    {
      aOutputMessage = string.Empty;
      Buffer.Add(aMessage);

      if (mErrorParser.LlvmIsMissing(aMessage))
      {
        MissingLLVM = true;
        return;
      }

      var text = String.Join("\n", Buffer) + "\n";
      if (mErrorParser.Detect(text, out Match aMatchResult))
      {
        GetOutputAndErrors(text, aHierarchy, out StringBuilder output, out List<TaskErrorModel> detectedErrors);

        aOutputMessage = output.ToString();
        Buffer.Clear();
        SaveErrorsMessages(detectedErrors);
      }
      else if (kBufferSize <= Buffer.Count)
      {
        aOutputMessage = Buffer[0];
        Buffer.RemoveAt(0);
      }
    }


    private void GetOutputAndErrors(string aText, IVsHierarchy aHierarchy, out StringBuilder aOutputBuilder, out List<TaskErrorModel> aDetectedErrors)
    {
      aOutputBuilder = new StringBuilder();
      aDetectedErrors = new List<TaskErrorModel>();

      while (mErrorParser.Detect(aText, out Match aMatchResult))
      {
        aDetectedErrors.Add(GetDetectedError(aHierarchy, aMatchResult));
        aOutputBuilder.Append(GetOutput(ref aText, aDetectedErrors.Last().FullMessage));
      }
    }


    private TaskErrorModel GetDetectedError(IVsHierarchy aHierarchy, Match aMarchResult)
    {
      IBuilder<TaskErrorModel> errorBuilder = new TaskErrorModelBuilder(aHierarchy, aMarchResult);
      errorBuilder.Build();
      return errorBuilder.GetResult();
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


    #endregion




  }
}
