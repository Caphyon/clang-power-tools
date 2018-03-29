using Microsoft.VisualStudio.Shell;
using System.Text.RegularExpressions;

namespace ClangPowerTools
{
  public class ErrorParser
  {
    #region Members

    private const string kErrorMessageRegex = @"(.\:\\[ \S+\\\/.]*[c|C|h|H|cpp|CPP|cc|CC|cxx|CXX|c++|C++|cp|CP])(\r\n|\r|\n| |:)*(\d+)(\r\n|\r|\n| |:)*(\d+)(\r\n|\r|\n| |:)*(error|note|warning)[^s](\r\n|\r|\n| |:)*(?<=[:|\r\n|\r|\n| ])(.*?)(?=[\[|\r\n|\r|\n])(.*)";

    #endregion

    #region Public Methods

    public bool FindErrors(string aMessages, out TaskError aError)
    {
      Regex regex = new Regex(kErrorMessageRegex);
      Match matchResult = regex.Match(aMessages);
      aError = null;

      if (!matchResult.Success)
        return false;

      var groups = matchResult.Groups;
      string messageDescription = groups[9].Value;

      if (string.IsNullOrWhiteSpace(messageDescription))
        return false;

      string path = groups[1].Value;
      int.TryParse(groups[3].Value, out int line);
      int.TryParse(groups[5].Value, out int column);

      string categoryAsString = groups[7].Value;
      TaskErrorCategory category = FindErrorCategory(ref categoryAsString);

      string clangTidyChecker = groups[10].Value;

      string fullMessage = CreateFullErrorMessage(path, line, categoryAsString, clangTidyChecker, messageDescription);

      messageDescription = messageDescription.Insert(0, ErrorParserConstants.kClangTag); // Add clang prefix for error list
      aError = new TaskError(path, line, column, category, messageDescription, fullMessage );

      return true;
    }

    private TaskErrorCategory FindErrorCategory(ref string aCategoryAsString)
    {
      TaskErrorCategory category;

      switch (aCategoryAsString)
      {
        case ErrorParserConstants.kErrorTag:
          category = TaskErrorCategory.Error;
          aCategoryAsString = ErrorParserConstants.kErrorTag;
          break;

        case ErrorParserConstants.kWarningTag:
          category = TaskErrorCategory.Warning;
          aCategoryAsString = ErrorParserConstants.kWarningTag;
          break;

        default:
          category = TaskErrorCategory.Message;
          aCategoryAsString = ErrorParserConstants.kMessageTag;
          break;
      }
      return category;
    }

    private string CreateFullErrorMessage(string aPath, int aLine, 
      string aCategory, string aClangTidyChecker, string aDescription)
    {
      return string.Format("{0}({1}): {2}{3}: {4}", aPath, aLine, aCategory,
        (true == string.IsNullOrWhiteSpace(aClangTidyChecker) ? string.Empty : $" {aClangTidyChecker.Trim(new char[] { ' ', '\n', '\r', '\t' })}"),
        aDescription);
    }

    public string Format(string aMessages, string aReplacement)
    {
      Regex regex = new Regex(kErrorMessageRegex);
      return regex.Replace(aMessages, aReplacement, 1);
    }

    public bool LlvmIsMissing(string aMessages)
    {
      return aMessages.Contains(ErrorParserConstants.kCompileClangMissingFromPath) ||
        aMessages.Contains(ErrorParserConstants.kTidyClangMissingFromPath);
    }

    #endregion

  }
}
