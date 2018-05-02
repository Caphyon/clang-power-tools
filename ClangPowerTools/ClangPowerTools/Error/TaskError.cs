using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace ClangPowerTools
{
  public class TaskError
  {
    #region Properties

    public string FilePath { get; set; }

    public int Line { get; set; }

    public int Column { get; set; }

    public TaskErrorCategory Category { get; set; }

    public string FullMessage { get; set; }

    public string Description { get; set; }

    public IVsHierarchy HierarchyItem { get; set; }

    #endregion

    #region Constructor

    public TaskError(string aFilePath, int aLine, int aColumn, 
      TaskErrorCategory aCategory, string aDescription, string aFullMessage)
    {
      FilePath = aFilePath;
      Line = aLine;
      Column = aColumn;
      Category = aCategory;
      Description = aDescription;
      FullMessage = aFullMessage;
    }

    #endregion

    #region Public Methods

    public override bool Equals(object obj)
    {
      var otherObj = obj as TaskError;
      if (null == otherObj)
        return false;

      return FullMessage.Replace('/', '\\') == otherObj.FullMessage.Replace('/', '\\');
    }

    public override int GetHashCode()
    {
      return $"{Line.ToString()}{FilePath}{FullMessage}".GetHashCode();
    }

    #endregion

  }
}
