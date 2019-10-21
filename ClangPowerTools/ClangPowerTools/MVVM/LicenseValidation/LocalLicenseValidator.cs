using ClangPowerTools.MVVM.Interfaces;

namespace ClangPowerTools.MVVM.LicenseValidation
{
  public class LocalLicenseValidator : LicenseValidator
  {
    #region ILicense Implementation

    public override bool Validate() => string.IsNullOrWhiteSpace(GetToken()) == false;

    #endregion

  }
}
