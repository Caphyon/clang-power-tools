using Microsoft.VisualStudio.Shell;

namespace ClangPowerTools
{
  public class TaskError
  {
    #region Properties

    public string Message { get; set; }
    public string FullMessage { get; set; }
    public string FilePath { get; set; }
    public int Line { get; set; }
    public TaskErrorCategory Category { get; set; } 

  #endregion

  #region Constructor

  public TaskError(string aFilePath, string aFullMessage, 
    string aMessage, int aLine, TaskErrorCategory aCategory)
    {
      FilePath = aFilePath;
      FullMessage = aFullMessage;
      Message = aMessage;
      Line = aLine;
      Category = aCategory;
    }

    #endregion

  }
}
