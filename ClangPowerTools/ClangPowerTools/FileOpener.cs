using EnvDTE80;

namespace ClangPowerTools
{
  public class FileOpener
  {
    #region Members

    private string kOpenCommand = "File.OpenFile";

    #endregion

    #region Public methods

    public void Open(DTE2 aDte, string aFilePath) => 
      aDte.ExecuteCommand(kOpenCommand, aFilePath);

    #endregion
  }
}
