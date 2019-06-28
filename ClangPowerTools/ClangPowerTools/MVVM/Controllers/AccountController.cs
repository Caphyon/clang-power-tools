using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ClangPowerTools.MVVM.WebApi;
using ClangPowerTools.Events;
using System.Collections.Generic;
using ClangPowerTools.MVVM.Models;

namespace ClangPowerTools.MVVM.Controllers
{
  public class AccountController
  {
    #region Members

    public static event EventHandler<LicenseEventArgs> OnLicenseStatusChanced;

    #endregion

    #region Public Methods

    public async Task<bool> LoginAsync(UserModel userModel)
    {
      StringContent content = new StringContent(SeralizeUserModel(userModel), Encoding.UTF8, "application/json");
      userModel.Dispose();

      try
      {
        using (HttpResponseMessage result = await ApiUtility.ApiClient.PostAsync(WebApiUrl.loginUrl, content))
        {
          content.Dispose();
          if (result.IsSuccessStatusCode)
          {
            TokenModel tokenModel = await result.Content.ReadAsAsync<TokenModel>();

            LicenseController licenseController = new LicenseController();
            bool licenseStatus = await licenseController.CheckLicenseAsync(tokenModel);

            if(licenseStatus == false)
            {
              return false;
            }

            SaveToken(tokenModel.jwt);
            OnLicenseStatusChanced.Invoke(this, new LicenseEventArgs(true));
            return true;
          }
          else
          {
            OnLicenseStatusChanced.Invoke(this, new LicenseEventArgs(false));
            return false;
          }
        }
      }
      catch (Exception)
      {
        OnLicenseStatusChanced.Invoke(this, new LicenseEventArgs(false));
        return false;
      }
    }

    #endregion

    #region Private Methods

    private void SaveToken(string token)
    {
      SettingsPathBuilder settingsPathBuilder = new SettingsPathBuilder();
      string filePath = settingsPathBuilder.GetPath("ctpjwt");
      DeleteExistingToken(filePath);

      using (StreamWriter streamWriter = new StreamWriter(filePath))
      {
        streamWriter.WriteLine(token);
      }
      File.SetAttributes(filePath, File.GetAttributes(filePath) | FileAttributes.Hidden);
    }

    private void DeleteExistingToken(string filePath)
    {
      if (File.Exists(filePath))
      {
        File.Delete(filePath);
      }
    }

    private string SeralizeUserModel(UserModel userModel)
    {
      string jsonObject = JsonConvert.SerializeObject(userModel);
      userModel.Dispose();
      return jsonObject;
    }

    #endregion

  }
}
