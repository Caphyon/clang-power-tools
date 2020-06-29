using ClangPowerTools.MVVM.LicenseValidation;
using System;

namespace ClangPowerTools.MVVM.Models
{
  public class AccountModel
  {
    #region Properties

    public string UserName { get; set; }
    public string Email { get; set; }

    public LicenseTypes LicenseType { get; set; }
    public DateTime LicenseExpirationDate { get; set; }

    public string Version { get; set; }

    #endregion

  }
}
