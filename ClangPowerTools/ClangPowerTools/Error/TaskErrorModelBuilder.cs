using ClangPowerTools.Builder;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Text.RegularExpressions;

namespace ClangPowerTools.Error
{
  public class TaskErrorModelBuilder : IBuilder<TaskErrorModel>
  {
    #region Members

    private TaskErrorModel mError = null;
    private Match mMatchResult = null;
    private IVsHierarchy mHierarchy;

    #endregion


    #region Consstructor


    public TaskErrorModelBuilder(IVsHierarchy aHierarchy, Match aMatchResult)
    {
      mMatchResult = aMatchResult;
      mHierarchy = aHierarchy;
    }

    #endregion


    #region IBuilder implementation


    public TaskErrorModel GetResult() => mError;


    public void Build()
    {
      var groups = mMatchResult.Groups;
      string messageDescription = groups[10].Value;

      if (string.IsNullOrWhiteSpace(messageDescription))
        return;

      string path = groups[1].Value;
      int.TryParse(groups[4].Value, out int line);
      int.TryParse(groups[6].Value, out int column);

      string categoryAsString = groups[8].Value;
      TaskErrorCategory category = FindErrorCategory(ref categoryAsString);

      string fullMessage = CreateFullErrorMessage(path, line, categoryAsString, messageDescription);

      // Add clang prefix for error list
      messageDescription = messageDescription.Insert(0, ErrorParserConstants.kClangTag);

      mError = new TaskErrorModel()
      {
        Document = path,
        Line = line - 1,
        Column = column,
        ErrorCategory = category,
        Text = messageDescription,
        FullMessage = fullMessage,
        HierarchyItem = mHierarchy,
        Category = TaskCategory.BuildCompile,
        Priority = TaskPriority.High
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


    private string CreateFullErrorMessage(string aPath, int aLine, string aCategory, string aDescription)
    {
      return $"{aPath}({aLine}): {aCategory}: {aDescription}";
    }

    #endregion


  }
}
