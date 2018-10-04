using ClangPowerTools.Services;
using EnvDTE;
using EnvDTE80;
using System.IO;

namespace ClangPowerTools
{
  public class FileOpener
  {
    #region Members

    private static readonly string kOpenCommand = "File.OpenFile";

    #endregion

    #region Public methods

    // Open the changed files in the editor
    public static void Open(object source, FileSystemEventArgs e)
    {
      if (VsServiceProvider.TryGetService(typeof(DTE), out object dte))
        (dte as DTE2).ExecuteCommand(kOpenCommand, e.FullPath);
    }

    #endregion
  }
}
