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
    #region Public Methods

    public async Task<bool> LoginAsync(UserModel userModel)
    {
      try
      {
        HttpResponseMessage userAccoutHttpRestul = await GetUserAccountHttpRestulAsync(userModel);

        if (userAccoutHttpRestul.IsSuccessStatusCode)
        {
          TokenModel tokenModel = await userAccoutHttpRestul.Content.ReadAsAsync<TokenModel>();

          if (string.IsNullOrWhiteSpace(tokenModel.jwt))
            return false;

          SaveToken(tokenModel.jwt);
          return true;
        }
        else
        {
          return false;
        }
      }
      catch (Exception)
      {
        return false;
      }
    }

    #endregion

    #region Private Methods

    private async Task<HttpResponseMessage> GetUserAccountHttpRestulAsync(UserModel userModel)
    {
      using StringContent content = new StringContent(SeralizeUserModel(userModel), Encoding.UTF8, "application/json");
      return await ApiUtility.ApiClient.PostAsync(WebApiUrl.loginUrl, content);
    }

    private void SaveToken(string token)
    {
      var settingsPathBuilder = new SettingsPathBuilder();
      string filePath = settingsPathBuilder.GetPath("ctpjwt");
      DeleteExistingToken(filePath);

      using var streamWriter = new StreamWriter(filePath);
      streamWriter.WriteLine(token);
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
