using Microsoft.VisualStudio.Shell;
using System.Text.RegularExpressions;

namespace ClangPowerTools.Error
{
  public class TaskErrorModelBuilder : IBuilder<TaskErrorModel>
  {
    #region Members

    private TaskErrorModel mError = null;
    private Match mMatchResult = null;

    #endregion


    #region Consstructor

    public TaskErrorModelBuilder(Match aMatchResult)
    {
      mMatchResult = aMatchResult;
    }


    #endregion


    #region IBuilder implementation


    public TaskErrorModel GetResult() => mError;


    public void Build()
    {
      var groups = mMatchResult.Groups;
      string messageDescription = groups[9].Value;

      if (string.IsNullOrWhiteSpace(messageDescription))
        return;

      string path = groups[1].Value;
      int.TryParse(groups[3].Value, out int line);
      int.TryParse(groups[5].Value, out int column);

      string categoryAsString = groups[7].Value;
      TaskErrorCategory category = FindErrorCategory(ref categoryAsString);

      string clangTidyChecker = groups[10].Value;

      string fullMessage = CreateFullErrorMessage(path, line, categoryAsString, clangTidyChecker, messageDescription);

      // Add clang prefix for error list
      messageDescription = messageDescription.Insert(0, ErrorParserConstants.kClangTag); 

      mError = new TaskErrorModel()
      {
        FilePath = path,
        Line = line,
        Column = column,
        Category = category,
        Description = messageDescription,
        FullMessage = fullMessage
      };

    }

    #endregion


    #region Private Methods

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

    #endregion


  }
}
