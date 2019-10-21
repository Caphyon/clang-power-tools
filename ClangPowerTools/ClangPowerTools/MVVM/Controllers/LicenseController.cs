using ClangPowerTools.Events;
using ClangPowerTools.Helpers;
using ClangPowerTools.MVVM.Models;
using ClangPowerTools.MVVM.WebApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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

    public async Task<bool> Validate()
    {
      bool networkConnection = await NetworkUtility.CheckInternetConnectionAsync();

      var token = GetToken();
      try
      {
        HttpResponseMessage userTokenHttpResponse = await GetTokenHttpResponseAsync(token);
        if (userTokenHttpResponse.IsSuccessStatusCode == false)
          return false;

        List<LicenseModel> licenses = await userTokenHttpResponse.Content.ReadAsAsync<List<LicenseModel>>();
      }

    #endregion
    }
  }
}
