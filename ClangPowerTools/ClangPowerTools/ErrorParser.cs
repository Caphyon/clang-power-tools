using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ClangPowerTools
{
  public class ErrorParser
  {
    #region Members

    private const string kCompileErrorsRegex = @"(.\:\\[ \w+\\\/.]*[h|cpp])(\r\n|\r|\n| |:)*(\d+)(\r\n|\r|\n| |:)*(\d+)(\r\n|\r|\n| |:)*(error|note|warning)[^s](\r\n|\r|\n| |:)*(.*)";

    #endregion

    #region Public Methods

    public bool FindErrors(string aMessages, out TaskError aError)
    {
      Regex regex = new Regex(kCompileErrorsRegex);
      Match matchResult = regex.Match(aMessages);
      aError = null;
      if (!matchResult.Success)
        return false;

      var groups = matchResult.Groups;
      string message = groups[9].Value;

      if (string.IsNullOrWhiteSpace(message))
        return false;

      string path = groups[1].Value;
      int.TryParse(groups[3].Value, out int line);
      string category = groups[7].Value;

      string fullMessage = string.Empty;
      TaskErrorCategory errorCategory;
      if (category == ErrorParserConstants.kErrorTag)
      {
        errorCategory = TaskErrorCategory.Error;
        fullMessage = $"{path}({line}): {ErrorParserConstants.kErrorTag}: {message}";
      }
      else if (category == ErrorParserConstants.kWarningTag)
      {
        errorCategory = TaskErrorCategory.Warning;
        fullMessage = $"{path}({line}): {ErrorParserConstants.kWarningTag}: {message}";
      }
      else
      {
        errorCategory = TaskErrorCategory.Message;
        fullMessage = $"{path}({line}): {ErrorParserConstants.kMessageTag}: {message}";
      }
      
      message = message.Insert(0, ErrorParserConstants.kClangTag);
      aError = new TaskError(path, fullMessage, message, line, errorCategory);
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
