using ClangPowerTools.Helpers;
using ClangPowerTools.MVVM.LicenseValidation;
using ClangPowerTools.MVVM.WebApi;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ClangPowerTools
{
  public class SettingsApi
  {
    #region Methods

    public async Task UploadSettingsAsync()
    {
      ApiUtility.InitializeApiClient();
      if (await NetworkUtility.CheckInternetConnectionAsync() == false) return;

      string settingsJson = "";
      HttpResponseMessage userAccoutHttpRestul = await GetHttpResponseAsync(settingsJson);

      if (userAccoutHttpRestul.IsSuccessStatusCode)
      {

      }

      //userTokenHttpResponse.

    }

    private static async Task<HttpResponseMessage> GetHttpResponseAsync(string settingsJson)
    {
      var token = new Token();
      token.GetToken(out string jwt);
      ApiUtility.ApiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
      string webApiUrl = string.Concat(WebApiUrl.settingsConfig, "/", settingsJson);
      return await ApiUtility.ApiClient.GetAsync(webApiUrl);
    }

    #endregion

  }
}
