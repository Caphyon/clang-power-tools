using ClangPowerTools.Helpers;
using ClangPowerTools.MVVM.LicenseValidation;
using ClangPowerTools.MVVM.WebApi;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerTools
{
  public class SettingsApi
  {
    #region Public Methods

    public async Task UploadSettingsAsync()
    {
      ApiUtility.InitializeApiClient();
      if (await NetworkUtility.CheckInternetConnectionAsync() == false) return;

      var settingsHandler = new SettingsHandler();
      string json = settingsHandler.GetSettingsAsJson();
      await PostSettingsAsync(json);
    }

    public async Task DownloadSettingsAsync()
    {
      ApiUtility.InitializeApiClient();
      if (await NetworkUtility.CheckInternetConnectionAsync() == false) return;


      HttpResponseMessage httpResponseMessage = await GetSettingsAsync();
      if (httpResponseMessage.IsSuccessStatusCode)
      {
        var settingsHandler = new SettingsHandler();
        settingsHandler.SaveSettings();
      }
    }

    #endregion

    #region Private Methods

    private async Task<HttpResponseMessage> PostSettingsAsync(string settingsJson)
    {
      SetAuthenticationHeader();

      using StringContent content = new StringContent(settingsJson, Encoding.UTF8, "application/json");
      return await ApiUtility.ApiClient.PostAsync(WebApiUrl.settingsConfig, content);
    }

    private async Task<HttpResponseMessage> GetSettingsAsync()
    {
      SetAuthenticationHeader();

      return await ApiUtility.ApiClient.GetAsync(WebApiUrl.settingsConfig);
    }

    private static void SetAuthenticationHeader()
    {
      var token = new Token();
      token.GetToken(out string jwt);
      ApiUtility.ApiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
    }

    #endregion

  }
}
