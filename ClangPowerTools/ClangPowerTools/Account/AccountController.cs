using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerTools
{
  public class AccountController
  {
    public static UserModel userModel = new UserModel();

    #region Readonly members
    private static readonly string appId = "5d011c6a375f6b5ed9716629";
    private static readonly string url = @"https://account.clangpowertools.com";
    private static readonly string loginUrl = string.Concat(url, "/api/", appId, "/user/", "login");
    private static readonly string licenseUrl = string.Concat(url, "/api/", appId, "/license");
    #endregion

    public static async Task<TokenModel> ApiCallAsync(string email, string password)
    {
      userModel = new UserModel(email, password);
      string jsonObject = JsonConvert.SerializeObject(userModel);
      var content = new StringContent(jsonObject, Encoding.UTF8, "application/json");

      try
      {
        using (HttpResponseMessage result = await ApiHelper.ApiClient.PostAsync(loginUrl, content))
        {
          if (result.IsSuccessStatusCode)
          {
            TokenModel tokenModel = await result.Content.ReadAsAsync<TokenModel>();
            ApiHelper.ApiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
            userModel.IsActive = true;
            return tokenModel;
          }
          else
          {
            throw new Exception(result.ReasonPhrase);
          }
        }
      }
      catch (Exception)
      {
        throw;
      }
    }

    public static async Task CheckLicenseAsync()
    {
      try
      {
        using (HttpResponseMessage result = await ApiHelper.ApiClient.GetAsync(licenseUrl))
        {
          if (result.IsSuccessStatusCode)
          {
            userModel.IsActive = true;
          }
          else
          {
            userModel.IsActive = false;
            throw new Exception(result.ReasonPhrase);
          }
        }
      }
      catch (Exception)
      {
        throw;
      }
    }
  }
}
