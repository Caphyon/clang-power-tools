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

    private ErrorDetector mErrorDetector = new ErrorDetector();
    private readonly int kBufferSize = 5;


    #endregion


    #region Methods


    #region Public methods


    public void ProcessData(string aMessage, IVsHierarchy aHierarchy, OutputContent aOutputContent)
    {
      aOutputContent.Buffer.Add(aMessage);
      
      if (mErrorDetector.LlvmIsMissing(aMessage))
      {
        aOutputContent.MissingLLVM = true;
        return;
      }

      var text = String.Join("\n", aOutputContent.Buffer) + "\n";
      if (mErrorDetector.Detect(text, out Match aMatchResult))
      {
        GetOutputAndErrors(text, aHierarchy, out StringBuilder output, out List<TaskErrorModel> aDetectedErrors);

        aOutputContent.Message = output.ToString();
        aOutputContent.Errors.UnionWith(aDetectedErrors);
        aOutputContent.Buffer.Clear();
      }
      else if (kBufferSize <= aOutputContent.Buffer.Count)
      {
        aOutputContent.Message = aOutputContent.Buffer[0];
        aOutputContent.Buffer.RemoveAt(0);
      }
    }

    #endregion


    #region Private Methods


    private void GetOutputAndErrors(string aText, IVsHierarchy aHierarchy, 
      out StringBuilder aOutputBuilder, out List<TaskErrorModel> aDetectedErrors)
    {
      aOutputBuilder = new StringBuilder();
      aDetectedErrors = new List<TaskErrorModel>();

      while (mErrorDetector.Detect(aText, out Match aMatchResult))
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


    #endregion


    #endregion

  }
}
