using ClangPowerTools.Helpers;
using System.Threading.Tasks;

namespace ClangPowerTools.MVVM.Controllers
{
  public class SettingsController
  {
    public SettingsController()
    {

      ApiUtility.InitializeApiClient();
    }

    public async Task UploadSettingsAsync()
    {
      if (await NetworkUtility.CheckInternetConnectionAsync() == false) return;

      //ApiUtility.ApiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
      //HttpResponseMessage userTokenHttpResponse = await ApiUtility.ApiClient.GetAsync(WebApiUrl.licenseUrl);

    }


  }
}
