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
    public const string kCompileErrorsRegex = @"(.\:\\[ \w+\\\/.]*[h|cpp])(\r\n|\r|\n| |:)*(\d+)(\r\n|\r|\n| |:)*(\d+)(\r\n|\r|\n| |:)*error(\r\n|\r|\n| |:)*(.*)";
    private IVsHierarchy mVsHierarchy;

    #endregion

    #region Properties

    public List<ScriptError> Errors => mErrors;

    #endregion

    #region Constructor

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

    #endregion

    #region Methods

    public bool Start(string aMessages)
    {
      if (LlvmIsMissing(aMessages))
        return false;

      Regex regex = new Regex(kCompileErrorsRegex);
      Match matchResult = regex.Match(aMessages);
      if (!matchResult.Success)
        return true;

      while (matchResult.Success)
      {
        var groups = matchResult.Groups;

        string path = groups[1].Value;
        int.TryParse(groups[3].Value, out int line);
        int.TryParse(groups[5].Value, out int column);
        string errorMessage = $"{ErrorParserConstants.kClangTag}{groups[8].Value}";
        //mErrors.Add(new ScriptError(mVsHierarchy, path, errorMessage, line, column));

        matchResult = matchResult.NextMatch();
      }
      return true;
    }

    private bool LlvmIsMissing(string aMessages)
    {
      return aMessages.Contains(ErrorParserConstants.kCompileClangMissingFromPath) ||
        aMessages.Contains(ErrorParserConstants.kTidyClangMissingFromPath) ?
        true : false;
    }

    #endregion

  }
}
