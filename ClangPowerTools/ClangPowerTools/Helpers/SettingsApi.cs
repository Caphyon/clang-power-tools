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
    #region Constructor

    public SettingsApi()
    {
      ApiUtility.InitializeApiClient();
    }

    #endregion

    #region Public Methods

    public async Task UploadSettingsAsync()
    {
      if (await NetworkUtility.CheckInternetConnectionAsync() == false) return;

      var settingsHandler = new SettingsHandler();
      string json = settingsHandler.GetSettingsAsJson();
      await PostSettingsAsync(json);
    }

    public async Task DownloadSettingsAsync()
    {
      if (await NetworkUtility.CheckInternetConnectionAsync() == false) return;

      HttpResponseMessage httpResponseMessage = await GetSettingsAsync();
      if (httpResponseMessage.IsSuccessStatusCode)
      {
        var settingsHandler = new SettingsHandler();
        string json = await httpResponseMessage.Content.ReadAsStringAsync();
        settingsHandler.LoadCloudSettings(json);
      }
    }

    public async Task<bool> CloudSaveExistsAsync()
    {
      if (await NetworkUtility.CheckInternetConnectionAsync() == false) return false;

      HttpResponseMessage httpResponseMessage = await GetSettingsAsync();
      if (httpResponseMessage.IsSuccessStatusCode)
      {
        return true;
      }

      return false;
    }

    public async Task<string> GetUserAccountDetailsJsonAsync()
    {
      if (await NetworkUtility.CheckInternetConnectionAsync() == false)
        return null;

      HttpResponseMessage httpResponseMessage = await GetSettingsAsync();
      if (!httpResponseMessage.IsSuccessStatusCode)
        return null;

      return await httpResponseMessage.Content.ReadAsStringAsync();
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

    private async Task<HttpResponseMessage> GetUserAccountDetailsAsync()
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
