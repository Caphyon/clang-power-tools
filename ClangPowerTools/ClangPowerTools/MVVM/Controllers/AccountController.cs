using ClangPowerTools.Events;
using ClangPowerTools.MVVM.WebApi;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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
      try
      {
        HttpResponseMessage userAccoutHttpRestul = await GetUserAccountHttpRestulAsync(userModel);

        if (userAccoutHttpRestul.IsSuccessStatusCode)
        {
          TokenModel tokenModel = await userAccoutHttpRestul.Content.ReadAsAsync<TokenModel>();

          LicenseController licenseController = new LicenseController();
          bool licenseStatus = await licenseController.CheckLicenseAsync(tokenModel);

          if (licenseStatus == false)
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
      catch (Exception)
      {
        OnLicenseStatusChanced.Invoke(this, new LicenseEventArgs(false));
        return false;
      }
    }

    public async Task<HttpResponseMessage> GetUserAccountHttpRestulAsync(UserModel userModel)
    {
      using StringContent content = new StringContent(SeralizeUserModel(userModel), Encoding.UTF8, "application/json");
      return await ApiUtility.ApiClient.PostAsync(WebApiUrl.loginUrl, content);
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
