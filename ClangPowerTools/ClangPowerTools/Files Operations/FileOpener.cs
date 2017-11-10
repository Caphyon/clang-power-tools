using EnvDTE80;
using System.IO;

namespace ClangPowerTools
{
  public class FileOpener
  {
    #region Members

    private string kOpenCommand = "File.OpenFile";
    private DTE2 mDte;

    #endregion

    #region Constructor

    public FileOpener(DTE2 aDte) => mDte = aDte;

    #endregion

    #region Public methods
    
    // Open the changed files in the editor
    public void FileChanged(object source, FileSystemEventArgs e) => 
      mDte.ExecuteCommand(kOpenCommand, e.FullPath);

    #endregion
  }
}
