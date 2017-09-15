using System;

namespace ClangPowerTools
{
  public class SilentFileChangerGuard : SilentFileChanger, IDisposable
  {
    #region Public methods

    public SilentFileChangerGuard(IServiceProvider aSite, string aDocument, bool aReloadDocument)
      : base(aSite, aDocument, aReloadDocument) => Suspend();

    public void Dispose() => Resume();

    #endregion
  }
}
