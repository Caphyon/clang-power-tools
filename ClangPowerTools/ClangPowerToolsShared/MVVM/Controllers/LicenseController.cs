using ClangPowerTools.MVVM.LicenseValidation;
using System.Threading.Tasks;

namespace ClangPowerTools.MVVM.Controllers
{
  public class LicenseController
  {

    #region Public Methods

    public async Task<LicenseType> GetUserLicenseTypeAsync()
    {
      if (await new CommercialLicenseValidator().ValidateAsync())
        return LicenseType.Commercial;

      if (await new PersonalLicenseValidator().ValidateAsync())
        return LicenseType.Personal;

      if (new FreeTrialController().IsActive())
        return LicenseType.Trial;

      if (await new LocalLicenseValidator().ValidateAsync())
        return LicenseType.SessionExpired;

      return LicenseType.NoLicense;
    }

    #endregion

  }
}
