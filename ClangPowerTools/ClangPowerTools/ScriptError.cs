using Microsoft.VisualStudio.Shell.Interop;

namespace ClangPowerTools
{
  public class ScriptError
  {
    #region Properties

    public string Message { get; set; }
    public string FullMessage { get; set; }

    public string FilePath { get; set; }
    public int Line { get; set; }
    public int Column { get; set; }
    public IVsHierarchy FileHierarchy { get; set; }

    #endregion

    #region Constructor

    public ScriptError(IVsHierarchy aHierarchy, string aFilePath, string aFullMessage, string aMessage, int aLine)
    {
      FileHierarchy = aHierarchy;
      FilePath = aFilePath;
      FullMessage = aFullMessage;
      Message = aMessage;
      Line = aLine;
    }
    #endregion

  }
}
