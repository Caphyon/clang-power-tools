using ClangPowerTools.Helpers;
using ClangPowerTools.MVVM.WebApi;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace ClangPowerTools.MVVM.Controllers
{
  public class LicenseController
  {
    #region Public Methods

    public async Task<bool> CheckLicenseAsync()
    {
      var networkAvailable = await NetworkUtility.CheckInternetConnectionAsync();

      if (networkAvailable)
      {
        return await CheckOnlineLicenseAsync();
      }
      else
      {
        return CheckLocalLicense();
      }
    }

    private bool CheckLocalLicense()
    {
      SettingsPathBuilder settingsPathBuilder = new SettingsPathBuilder();
      string filePath = settingsPathBuilder.GetPath("ctpjwt");
      return File.Exists(filePath);
    }


    private async Task<bool> CheckOnlineLicenseAsync()
    {
      try
      {
        // TODO check license API code
        using (HttpResponseMessage result = await ApiUtility.ApiClient.GetAsync(WebApiUrl.licenseUrl))
        {
          if (result.IsSuccessStatusCode)
          {
            return true;
          }
          else
          {
            return false;
          }
        }
      }
      catch (Exception)
      {
        return false;
      }
    }

    #endregion
  }
}
