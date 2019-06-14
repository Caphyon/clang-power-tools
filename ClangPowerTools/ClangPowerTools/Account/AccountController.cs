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
    private static readonly string appId = "5d011c6a375f6b5ed9716629";
    private static readonly string url = @"https://account.clangpowertools.com";

    #region Commands
    private static readonly string loginUrl = string.Concat(url, "/api/", appId, "/user/", "login");
    private static readonly string licenseUrl = string.Concat(url, "/api/", appId, "/license");
    #endregion

    internal static async Task<TokenModel> ApiCallAsync(string email, string password)
    {
      UserModel userModel = new UserModel(email, password);
      string jsonObject = JsonConvert.SerializeObject(userModel);
      var content = new StringContent(jsonObject, Encoding.UTF8, "application/json");
      TokenModel tokenModel = new TokenModel();

      try
      {
        using (HttpResponseMessage result = await ApiHelper.ApiClient.PostAsync(loginUrl, content))
        {
          if (result.IsSuccessStatusCode)
          {
            tokenModel = await result.Content.ReadAsAsync<TokenModel>();
            ApiHelper.ApiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
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

    internal static async Task CheckLicenseAsync()
    {
      try
      {
        using (HttpResponseMessage result = await ApiHelper.ApiClient.GetAsync(licenseUrl))
        {
          if (result.IsSuccessStatusCode)
          {
            var test = result.Content;
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
  }
}
