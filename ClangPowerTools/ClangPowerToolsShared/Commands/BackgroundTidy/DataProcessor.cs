using ClangPowerTools.Builder;
using ClangPowerTools.Error;
using ClangPowerTools.Output;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace ClangPowerTools.Commands.BackgroundTidy
{
  public class DataProcessor
  {
    #region Members 

    private readonly ErrorDetector errorDetector = new ErrorDetector();
    private readonly OutputContentModel outputContent = new OutputContentModel();

    private readonly int kBufferSize = 5;

    #endregion


    #region Methods


    #region Public methods

    public void ReceiveData(object sender, DataReceivedEventArgs e)
    {
      if (null == e.Data)
        return;

      if (outputContent.HasEncodingError)
        return;

      Process(e.Data);
    }

    public void ClosedDataConnection(object sender, EventArgs e)
    {
      if (outputContent.Errors.Count <= 0)
        return;

      TaskErrorViewModel.FileErrorsPair = new Dictionary<string, List<TaskErrorModel>>();
      foreach (var error in outputContent.Errors)
      {
        if (TaskErrorViewModel.FileErrorsPair.ContainsKey(error.Document))
        {
          TaskErrorViewModel.FileErrorsPair[error.Document]
            .Add(error);
        }
        else
        {
          TaskErrorViewModel.FileErrorsPair
            .Add(error.Document, new List<TaskErrorModel>() { error });
        }
      }

    }

    #endregion


    #region Private Methods

    private void Process(string message)
    {
      outputContent.Buffer.Add(message);

      var text = String.Join("\n", outputContent.Buffer.ToList()) + "\n";
      if (errorDetector.Detect(text, ErrorParserConstants.kErrorMessageRegex, out _))
      {
        GetErrors(text, null, out List<TaskErrorModel> aDetectedErrors);
        outputContent.Errors.UnionWith(aDetectedErrors);
        outputContent.Buffer.Clear();
      }
      else if (kBufferSize <= outputContent.Buffer.Count)
      {
        outputContent.Buffer.RemoveAt(0);
      }
    }

    private void GetErrors(string text, IVsHierarchy hierarchy, out List<TaskErrorModel> detectedErrors)
    {
      detectedErrors = new List<TaskErrorModel>();

      while (errorDetector.Detect(text, ErrorParserConstants.kErrorMessageRegex, out Match aMatchResult))
      {
        detectedErrors.Add(GetDetectedError(hierarchy, aMatchResult));
        GetOutput(ref text, detectedErrors.Last().FullMessage);
      }
    }

    private void GetOutput(ref string aText, string aSearchedSubstring)
    {
      var errorFormatter = new ErrorFormatter();
      aText = errorFormatter.Format(aText, aSearchedSubstring);
      aText = aText.SubstringAfter(aSearchedSubstring);
    }

    private TaskErrorModel GetDetectedError(IVsHierarchy aHierarchy, Match aMarchResult)
    {
      IBuilder<TaskErrorModel> errorBuilder = new TaskErrorModelBuilder(aHierarchy, aMarchResult);
      errorBuilder.Build();
      return errorBuilder.GetResult();
    }

    #endregion

    #endregion

  }
}
