using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ClangPowerTools
{
  public class ErrorParser
  {
    #region Members

    private List<ScriptError> mErrors = new List<ScriptError>();

    private string mErrorfilePath = string.Empty;
    private string mErrorMessage = ErrorParserConstants.kClangTag;
    private int[] mErrorPosition = new int[2];
    private IVsHierarchy mVsHierarchy;

    #endregion

    #region Properties

    public List<ScriptError> Errors => mErrors;

    #endregion

    #region Ctor

    public ErrorParser(IServiceProvider aServiceProvider, IItem aItem)
    {
      if (aItem is SelectedProjectItem)
      {
        ProjectItem projectItem = aItem.GetObject() as ProjectItem;
        mVsHierarchy = AutomationUtil.GetProjectHierarchy(aServiceProvider, projectItem.ContainingProject);
      }
      else
      {
        Project project = aItem.GetObject() as Project;
        mVsHierarchy = AutomationUtil.GetProjectHierarchy(aServiceProvider, project);
      }
    }

    public ErrorParser() { }

    #endregion

    public bool Start(List<string> aMessages)
    {
      bool isError = false;
      bool pathFound = false;
      bool positionFound = false;
      bool skipSearchPath = false;

      foreach (string message in aMessages)
      {
        string errorMessage = message;

        if (string.IsNullOrWhiteSpace(errorMessage))
          continue;

        if (errorMessage.Contains(ErrorParserConstants.kCompileClangMissingFromPath) ||
          errorMessage.Contains(ErrorParserConstants.kTidyClangMissingFromPath))
        {
          return false;
        }

        if (errorMessage.Contains(ErrorParserConstants.kEndErrorsTag))
          break;

        if (errorMessage.StartsWith(ErrorParserConstants.kErrorTag))
        {
          errorMessage = errorMessage.Substring(ErrorParserConstants.kErrorTag.Length);
          isError = true;
        }

        if (isError == false)
          continue;

        if (pathFound == false)
        {
          if (!FindPath(errorMessage, ref mErrorfilePath, ref pathFound, ref positionFound, ref skipSearchPath, false))
            continue;
        }

        if (positionFound == false)
        {
          if (!FindPosition(errorMessage, ref positionFound))
            continue;
        }

        if (skipSearchPath == false)
        {
          if (FindPath(errorMessage, ref mErrorfilePath, ref pathFound, ref positionFound, ref skipSearchPath, true))
          {
            if (!FindPosition(errorMessage, ref positionFound))
              continue;

            skipSearchPath = true;
            FindErrorMessage(errorMessage);
            continue;
          }
          else
          {
            mErrorMessage = $"{mErrorMessage}\n{errorMessage}";
            continue;
          }
        }

        skipSearchPath = false;
        FindErrorMessage(errorMessage);
      }
      if (isError)
      {
        mErrors.Add(new ScriptError(mVsHierarchy, mErrorfilePath, mErrorMessage, mErrorPosition[0], mErrorPosition[1]));
        mErrorMessage = ErrorParserConstants.kClangTag;
      }
      mErrors.RemoveAll(err => ErrorParserConstants.kClangTag == err.ErrorMessage);
      return true;
    }

    private bool FindPath(string aOutputMessage, ref string aErrorFilePath, ref bool aPathFound,
      ref bool aPositionFound, ref bool aSkipSearchPath, bool aAddError)
    {
      Regex regex = new Regex(RegexConstants.kFindAllPaths);
      Match matchResult = regex.Match(aOutputMessage);
      if (!matchResult.Success)
        return false;

      if (aAddError)
      {
        mErrors.Add(new ScriptError(mVsHierarchy, aErrorFilePath, mErrorMessage, mErrorPosition[0], mErrorPosition[1]));
        mErrorMessage = ErrorParserConstants.kClangTag;
      }

      aErrorFilePath = matchResult.Value;
      aPathFound = true;
      aPositionFound = false;
      aSkipSearchPath = true;

      return true;
    }

    private bool FindPosition(string aOutputMessage, ref bool aPositionFound)
    {
      Regex regex = new Regex(RegexConstants.kFindLineAndColumn);
      Match matchResult = regex.Match(aOutputMessage);

      if (!matchResult.Success)
        return false;

      aPositionFound = true;
      int index = 0;

      while (matchResult.Success)
      {
        int.TryParse(matchResult.Value, out mErrorPosition[index++]);
        matchResult = matchResult.NextMatch();
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
        mErrorMessage = $"{mErrorMessage}{aOutputMessage.Substring(message + ErrorParserConstants.kNoteTag.Length)}";
      }
      else
        mErrorMessage = $"{mErrorMessage}{aOutputMessage.Substring(message + ErrorParserConstants.kErrorTag.Length)}";

      return true;
    }

  }
}
