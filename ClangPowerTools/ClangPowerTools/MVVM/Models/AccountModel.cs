using ClangPowerTools.MVVM.LicenseValidation;

namespace ClangPowerTools.MVVM.Models
{
  public class AccountModel
  {
    #region Properties

    public string UserName { get; set; }
    public string Email { get; set; }

    public LicenseType LicenseType { get; set; }
    public string LicenseExpirationDate { get; set; }

    #endregion

  }
}
