using Microsoft.VisualStudio.Shell.Interop;

namespace ClangPowerTools.SilentFile
{
  public class SilentFileChangerModel
  {
    #region Members


    public string DocumentFileName { get; set; }
    public IVsPersistDocData PersistDocData { get; set; }
    public IVsDocDataFileChangeControl FileChangeControl { get; set; }
    public bool IsSuspended { get; set; }


    #endregion

  }
}
