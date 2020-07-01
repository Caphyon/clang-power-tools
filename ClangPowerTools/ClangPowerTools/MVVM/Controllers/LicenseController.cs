using ClangPowerTools.Events;
using ClangPowerTools.Helpers;
using ClangPowerTools.MVVM.LicenseValidation;
using System;
using System.Threading.Tasks;

namespace ClangPowerTools.MVVM.Controllers
{
  public class LicenseController
  {
    #region Members

    public static event EventHandler<LicenseEventArgs> OnLicenseStatusChanced;

    #endregion

    #region Public Methods

    public async Task<bool> CheckLicenseAsync()
    {
      bool networkConnection = await NetworkUtility.CheckInternetConnectionAsync();

      bool tokenExists = await new LocalLicenseValidator().ValidateAsync();
      bool licenseStatus = networkConnection ? await new PersonalLicenseValidator().ValidateAsync() : tokenExists;

      OnLicenseStatusChanced.Invoke(this, new LicenseEventArgs(licenseStatus, tokenExists));
      return licenseStatus;
    }

    public async Task<LicenseType> GetUserLicenseTypeAsync()
    {
      if (await new CommercialLicenseValidator().ValidateAsync())
        return LicenseType.Commercial;

      if (await new PersonalLicenseValidator().ValidateAsync())
        return LicenseType.Personal;

      return LicenseType.Trial;
    }

    #endregion

  }
}
