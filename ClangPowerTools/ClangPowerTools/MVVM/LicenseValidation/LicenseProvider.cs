namespace ClangPowerTools.MVVM.LicenseValidation
{
  public class LicenseProvider
  {
    #region Properties

    public LicenseInfo License { get; private set; }

    #endregion

    #region Constructor

    public LicenseProvider(LicenseInfo licenseInfo)
    {
      License = licenseInfo;
    }

    #endregion

  }
}
