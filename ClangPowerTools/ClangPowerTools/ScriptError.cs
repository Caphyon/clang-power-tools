using Microsoft.VisualStudio.Shell.Interop;

namespace ClangPowerTools
{
  public class ScriptError
  {
    #region Properties

    public string ErrorMessage { get; set; }
    public string FilePath { get; set; }
    public int Line { get; set; }
    public int Column { get; set; }
    public IVsHierarchy FileHierarchy { get; set; }
    
    #endregion

    #region Ctor

    public ScriptError(IVsHierarchy aHierarchy, string aFilePath, string aError, int aLine, int aColumn)
    {
      FileHierarchy = aHierarchy;
      FilePath = aFilePath;
      ErrorMessage = aError;
      Line = aLine;
      Column = aColumn;
    }
    #endregion

  }
}
