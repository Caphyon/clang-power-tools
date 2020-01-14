using ClangPowerTools.Events;
using ClangPowerTools.Helpers;
using System;
using System.Threading.Tasks;

namespace ClangPowerTools.MVVM.Controllers
{
  public class LicenseController
  {
    #region Members

    public static event EventHandler<LicenseEventArgs> OnLicenseStatusChanced;
    private readonly string fileName = "ctpjwt";

    #endregion

    #region Public Methods

    public async Task<bool> CheckLicenseAsync()
    {
      bool networkConnection = await NetworkUtility.CheckInternetConnectionAsync();
      return true;

      //var token = GetToken();
      //try
      //{
      //  HttpResponseMessage userTokenHttpResponse = await GetTokenHttpResponseAsync(token);
      //  if (userTokenHttpResponse.IsSuccessStatusCode == false)
      //    return false;

      //  List<LicenseModel> licenses = await userTokenHttpResponse.Content.ReadAsAsync<List<LicenseModel>>();
      //}

    }

    #endregion

  }
}
