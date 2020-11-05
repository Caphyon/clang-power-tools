using ClangPowerTools.MVVM.LicenseValidation;

namespace ClangPowerTools.MVVM.Models
{
  public class AccountModel
  {
    #region Properties

    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public LicenseType LicenseType { get; set; } = LicenseType.NoLicense;
    public string LicenseExpirationDate { get; set; } = "Never";

    #endregion

  }
}
