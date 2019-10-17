using ClangPowerTools.MVVM.Interfaces;
using ClangPowerTools.MVVM.Models;
using ClangPowerTools.MVVM.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ClangPowerTools.MVVM.LicenseValidation
{
  /// <summary>
  /// Contains the logic for the user license verification.
  /// </summary>
  public class CommercialLicenseValidator : LocalLicenseValidator, ILicenseValidator
  {
    #region ILicenseValidator Implementation

    /// <summary>
    /// Verify if the user license is active.
    /// </summary>
    /// <returns>True if the user license is active. False otherwise.</returns>
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

    #endregion

    #region Private Methods

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
      return licenses.Any(license => CheckExpirationDate(license.expires)) == false;
    }

    /// <summary>
    /// Check if the license expired
    /// </summary>
    /// <param name="expirationDate">The expiration date as string</param>
    /// <returns>True if the license expired. False otherwise.</returns>
    private bool CheckExpirationDate(string expirationDate)
    {
      DateTime.TryParse(expirationDate, out DateTime expirationDateTime);
      return DateTime.Compare(DateTime.Now, expirationDateTime) < 0;
    }

    #endregion

  }
}
