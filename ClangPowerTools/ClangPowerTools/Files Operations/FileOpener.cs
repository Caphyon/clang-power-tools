using EnvDTE80;
using System.IO;

namespace ClangPowerTools
{
  public class FileOpener
  {
    #region Members

    private static readonly string kOpenCommand = "File.OpenFile";
    private static DTE2 mDte;

    #endregion

    #region Constructor

    public static void Initialize(DTE2 aDte) => mDte = aDte;

    #endregion


    #region Public methods
    
    // Open the changed files in the editor
    public static void Open(object source, FileSystemEventArgs e) => 
      mDte.ExecuteCommand(kOpenCommand, e.FullPath);

    #endregion
  }
}
