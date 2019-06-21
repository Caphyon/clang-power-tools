using ClangPowerTools.Events;
using ClangPowerTools.Helpers;
using ClangPowerTools.MVVM.WebApi;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
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
      var networkAvailable = await NetworkUtility.CheckInternetConnectionAsync();
      bool licenceStatus;

      if (networkAvailable)
      {
        licenceStatus = await CheckOnlineLicenseAsync();
      }
      else
      {
        licenceStatus = CheckLocalLicense();
      }

      OnLicenseStatusChanced.Invoke(this, new LicenseEventArgs(licenceStatus));
      return licenceStatus;
    }

    private bool CheckLocalLicense()
    {
      SettingsPathBuilder settingsPathBuilder = new SettingsPathBuilder();
      string filePath = settingsPathBuilder.GetPath("ctpjwt");
      return File.Exists(filePath);
    }


    private async Task<bool> CheckOnlineLicenseAsync()
    {
      TokenModel tokenModel = CheckToken();

      if(ApiUtility.ApiClient == null)
      {
        ApiUtility.InitializeApiClient();
      }

      if(tokenModel.jwt == "")
      {
        return false;
      }

      try
      {
        ApiUtility.ApiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.jwt);
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


    private TokenModel CheckToken()
    {
      SettingsPathBuilder settingsPathBuilder = new SettingsPathBuilder();
      string filePath = settingsPathBuilder.GetPath("ctpjwt");
      TokenModel tokenModel = new TokenModel();

      if (File.Exists(filePath))
      {
        using (StreamReader streamReader = new StreamReader(filePath))
        {
          tokenModel.jwt = streamReader.ReadLine();
        }
      }
      return tokenModel;
    }


    #endregion
  }
}
