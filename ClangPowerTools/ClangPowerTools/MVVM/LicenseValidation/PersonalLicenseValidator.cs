using ClangPowerTools.MVVM.Models;
using ClangPowerTools.MVVM.WebApi;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ClangPowerTools.MVVM.LicenseValidation
{
  public class PersonalLicenseValidator : LocalLicenseValidator, ILicenseValidator
  {
    public async Task<bool> ValidateAsync()
    {
      var token = GetToken();
      try
      {
        HttpResponseMessage userTokenHttpResponse = await GetTokenHttpResponseAsync(token);
        if (userTokenHttpResponse.IsSuccessStatusCode == false)
          return false;
        
        List<LicenseModel> licenses = await userTokenHttpResponse.Content.ReadAsAsync<List<LicenseModel>>();
        return VerifyLicense(licenses);
      }
      catch (Exception)
      {
        return false;
      }
    }

    /// <summary>
    /// Get the HTTP server response for user token
    /// </summary>
    /// <param name="token">The user license token</param>
    /// <returns>The HTTP server response for the given token</returns>
    private async Task<HttpResponseMessage> GetTokenHttpResponseAsync(string token)
    {
      if (ApiUtility.ApiClient == null)
        ApiUtility.InitializeApiClient();

      ApiUtility.ApiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
      return await ApiUtility.ApiClient.GetAsync(WebApiUrl.licenseUrl);
    }

    /// <summary>
    /// Verify if at least one of the user licenses is active. 
    /// </summary>
    /// <param name="userTokenHttpResponse">The HTTP server response for user token</param>
    /// <returns>True if at least one of the user licenses is active. False otherwise.</returns>
    private bool VerifyLicense(List<LicenseModel> licenses)
    {
      return licenses.Count == 0;
    }

  }
}
