using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Text.RegularExpressions;

namespace ClangPowerTools
{
  public class ErrorParser
  {
    #region Members

    private List<ScriptError> mErrors = new List<ScriptError>();
    private List<string> mOutput = new List<string>();

    private IVsHierarchy mHierarchy;
    private string mErrorfilePath = string.Empty;
    private string mErrorMessage = string.Empty;
    private int[] mErrorPosition = new int[2];

    #endregion

    #region Properties

    public List<ScriptError> Errors => mErrors;
    public List<string> Output => mOutput;

    #endregion

    #region Ctor

    public ErrorParser(IVsHierarchy aHierarchy) => mHierarchy = aHierarchy;

    #endregion

    public void Start(Collection<PSObject> aOutput)
    {
      bool isError = false;
      bool pathFound = false;
      bool positionFound = false;
      bool skipSearchPath = false;

      try
      {
        foreach (PSObject outObj in aOutput)
        {
          string outputMessage = outObj.BaseObject.ToString();
          mOutput.Add(outputMessage);

          if (string.IsNullOrWhiteSpace(outputMessage))
            continue;

          if (outputMessage.Contains(ErrorParserConstants.kEndErrorsTag))
            break;

          if (outputMessage.StartsWith(ErrorParserConstants.kErrorTag))
          {
            outputMessage = outputMessage.Substring(ErrorParserConstants.kErrorTag.Length);
            isError = true;
          }

          if (isError == false)
            continue;

          if (pathFound == false)
          {
            if (!FindPath(outputMessage, ref mErrorfilePath, ref pathFound, ref positionFound, ref skipSearchPath, false))
              continue;
          }

          if (positionFound == false)
          {
            if (!FindPosition(outputMessage, ref positionFound))
              continue;
          }

          if (skipSearchPath == false)
          {
            if (FindPath(outputMessage, ref mErrorfilePath, ref pathFound, ref positionFound, ref skipSearchPath, true))
            {
              if (!FindPosition(outputMessage, ref positionFound))
                continue;

              skipSearchPath = true;
              FindErrorMessage(outputMessage);
              continue;
            }
            else
            {
              mErrorMessage = $"{mErrorMessage}\n{outputMessage}";
              continue;
            }
          }

          skipSearchPath = false;
          FindErrorMessage(outputMessage);
        }
        if (isError)
          mErrors.Add(new ScriptError(mHierarchy, mErrorfilePath, mErrorMessage, mErrorPosition[0], mErrorPosition[1]));

        mErrors.RemoveAll(err => String.IsNullOrWhiteSpace(err.ErrorMessage));
      }
      catch(Exception exception)
      {
        VsShellUtilities.ShowMessageBox((IServiceProvider)this, exception.Message, "Error",
          OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
      }

    }

    private bool FindPath(string aOutputMessage, ref string aErrorfilePath, ref bool aPathFound, 
      ref bool aPositionFound, ref bool aSkipSearchPath, bool aAddError)
    {
      Regex regex = new Regex(RegexConstants.kFindAllPaths);
      Match matchResult = regex.Match(aOutputMessage);
      if (!matchResult.Success)
        return false;

      if (aAddError)
        mErrors.Add(new ScriptError(mHierarchy, aErrorfilePath, mErrorMessage, mErrorPosition[0], mErrorPosition[1]));

      aErrorfilePath = matchResult.Value;
      aPathFound = true;
      aPositionFound = false;
      aSkipSearchPath = true;

      return true;
    }

    private bool FindPosition(string aOutputMessage, ref bool aPositionFound)
    {
      Regex regex = new Regex(RegexConstants.kFindAllNumbers);
      Match matchResult = regex.Match(aOutputMessage);
      if (!matchResult.Success)
        return false;

      aPositionFound = true;
      int index = 0;

      while (matchResult.Success)
      {
        int.TryParse(matchResult.Value, out mErrorPosition[index++]);
        matchResult = matchResult.NextMatch();
        if (1 < index)
          break;
      }
      return true;
    }

    private bool FindErrorMessage(string aOutputMessage)
    {
      int message = aOutputMessage.IndexOf(ErrorParserConstants.kErrorTag.ToLower());
      if (-1 == message)
      {
        message = aOutputMessage.IndexOf(ErrorParserConstants.kNoteTag);
        if (-1 == message)
          return false;
        mErrorMessage = aOutputMessage.Substring(message + ErrorParserConstants.kNoteTag.Length);
      }
      else
        mErrorMessage = aOutputMessage.Substring(message + ErrorParserConstants.kErrorTag.Length);

      return true;
    }

  }
}
