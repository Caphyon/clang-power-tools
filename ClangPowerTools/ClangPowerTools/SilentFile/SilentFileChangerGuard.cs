using ClangPowerTools.SilentFile;
using System;
using System.Collections.Generic;

namespace ClangPowerTools
{
  public class SilentFileChangerGuard : SilentFileChanger, IDisposable
  {
    #region Members

    private HashSet<SilentFileChanger> mFileChangers = 
      new HashSet<SilentFileChanger>(new SilentFileChangerEqualityComparer());

    #endregion

    #region Constructor

    public SilentFileChangerGuard() { }

    #endregion

    #region Public methods

    public SilentFileChangerGuard(IServiceProvider aSite, string aDocument, bool aReloadDocument)
      : base(aSite, aDocument, aReloadDocument) => Suspend();

    public void Add(SilentFileChanger aFileChanger)
    {
      aFileChanger.Suspend();
      mFileChangers.Add(aFileChanger);
    }

    public void AddRange(IServiceProvider aServiceProvider, List<string> aFilesPath)
    {
      foreach (string filePath in aFilesPath)
        this.Add(new SilentFileChanger(aServiceProvider, filePath, true));
    }

    public void Dispose()
    {
      foreach (SilentFileChanger file in mFileChangers)
        file.Resume();
      Resume();
    }
    #endregion
  }
}
