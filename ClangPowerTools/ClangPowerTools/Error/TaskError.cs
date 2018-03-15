using Microsoft.VisualStudio.Shell;

namespace ClangPowerTools
{
  public class TaskError
  {
    #region Properties

    public string FilePath { get; set; }

    public int Line { get; set; }

    public TaskErrorCategory Category { get; set; }

    public string FullMessage { get; set; }

    public string Description { get; set; }

    #endregion

    #region Constructor

    public TaskError(string aFilePath, string aFullMessage,
      string aMessage, int aLine, TaskErrorCategory aCategory)
    {
      FilePath = aFilePath;
      Line = aLine;
      Category = aCategory;
      Description = aMessage;
      FullMessage = aFullMessage;
    }

    #endregion

    #region Public Methods

    public override bool Equals(object obj)
    {
      var otherObj = obj as TaskError;
      if (null == otherObj)
        return false;

      return Line == otherObj.Line &&
        FilePath == otherObj.FilePath &&
        FullMessage == otherObj.FullMessage;
    }

    public override int GetHashCode()
    {
      return $"{Line.ToString()}{FilePath}{FullMessage}".GetHashCode();
    }

    #endregion

  }
}
