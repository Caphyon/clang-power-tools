using ClangPowerTools.Events;
using ClangPowerTools.Helpers;
using ClangPowerTools.MVVM.Models;
using ClangPowerTools.MVVM.WebApi;
using System;
using System.Collections.Generic;
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

    public async Task<bool> CheckLicenseAsync(TokenModel tokenModel = null)
    {
      var networkAvailable = await NetworkUtility.CheckInternetConnectionAsync();
      if(tokenModel == null)
      {
        tokenModel = new TokenModel();
      }
      bool licenceStatus = networkAvailable ? await CheckOnlineLicenseAsync(tokenModel) : CheckLocalLicense();

      OnLicenseStatusChanced.Invoke(this, new LicenseEventArgs(licenceStatus));
      return licenceStatus;
    }

    private bool CheckLocalLicense()
    {
      SettingsPathBuilder settingsPathBuilder = new SettingsPathBuilder();
      string filePath = settingsPathBuilder.GetPath("ctpjwt");
      return File.Exists(filePath);
    }


    private async Task<bool> CheckOnlineLicenseAsync(TokenModel tokenModel)
    {
      tokenModel = CheckToken(tokenModel);

      if (ApiUtility.ApiClient == null)
      {
        ApiUtility.InitializeApiClient();
      }

      if (tokenModel.jwt == "")
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
            return await CheckAllLicensesAsync(result);
          }
          else
          {
            return true;
          }
        }
      }
      catch (Exception)
      {
        return false;
      }
    }

    private async Task<bool> CheckAllLicensesAsync(HttpResponseMessage result)
    {
      List<LicenseModel> licenses = await result.Content.ReadAsAsync<List<LicenseModel>>();
      if (licenses.Count == 0)
      {
        return true;
      }

      foreach (LicenseModel item in licenses)
      {
        if (item.active == true)
        {
          return true;
        }
      }
      return false;
    }

    private TokenModel CheckToken(TokenModel tokenModel)
    {
      SettingsPathBuilder settingsPathBuilder = new SettingsPathBuilder();
      string filePath = settingsPathBuilder.GetPath("ctpjwt");

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
