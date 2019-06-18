using ClangPowerTools.Helpers;
using ClangPowerTools.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerTools.MVVM.Controllers
{
  public class LicenseController
  {
    #region Public Methods

    public async Task<bool> CheckLicenseAsync()
    {
      var accountController = new AccountController();
      var networkAvailable = await NetworkUtility.CheckInternetConnectionAsync();

      if (networkAvailable)
      {
        await accountController.CheckLicenseAsync();
      }
      else
      {
        accountController.CheckLocalLicense();
      }

      return accountController.GetUserModel().IsActive;
    }

    #endregion

  }
}
