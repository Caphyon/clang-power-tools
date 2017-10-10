using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ClangPowerTools
{
  public class ErrorCreator
  {
    #region Members

    private const string kCompileErrorsRegex = @"(.\:\\[ \w+\\\/.]*[h|cpp])(\r\n|\r|\n| |:)*(\d+)(\r\n|\r|\n| |:)*(\d+)(\r\n|\r|\n| |:)*error(\r\n|\r|\n| |:)*(.*)";
    private IVsHierarchy mVsHierarchy;

    #endregion

    #region Public Methods

    public void FindHierarchy(IServiceProvider aServiceProvider, IItem aItem)
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

    public bool FindErrors(string aMessages, out ScriptError aError)
    {
      Regex regex = new Regex(kCompileErrorsRegex);
      Match matchResult = regex.Match(aMessages);
      aError = null;
      if (!matchResult.Success)
        return false;

      var groups = matchResult.Groups;

      string path = groups[1].Value;
      int.TryParse(groups[3].Value, out int line);
      int.TryParse(groups[5].Value, out int column);
      string message = $"{ErrorParserConstants.kClangTag}{groups[8].Value}";
      string fullMessage = $"{path}({line},{column}): error: {groups[8].Value}".ToLower();

      aError = new ScriptError(mVsHierarchy, path, fullMessage, message, line, column);

      return true;
    }

    public string Format(string aMessages, string aReplacement)
    {
      Regex regex = new Regex(kCompileErrorsRegex);
      return regex.Replace(aMessages, aReplacement);
    }

    public bool LlvmIsMissing(string aMessages)
    {
      return aMessages.Contains(ErrorParserConstants.kCompileClangMissingFromPath) ||
        aMessages.Contains(ErrorParserConstants.kTidyClangMissingFromPath) ?
        true : false;
    }

    #endregion

  }
}
