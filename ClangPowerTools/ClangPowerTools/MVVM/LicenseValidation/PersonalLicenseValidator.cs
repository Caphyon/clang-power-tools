using ClangPowerTools.MVVM.WebApi;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ClangPowerTools.MVVM.LicenseValidation
{
  public class PersonalLicenseValidator : LicenseValidator
  {
    public override async Task<bool> ValidateAsync()
    {
      try
      {
        if (GetToken(out string token) == false)
          return false;

        KeyValuePair<bool, HttpResponseMessage> httpResponse = await CheckUserAccountAsync(token);
        return httpResponse.Key;
      }
      catch (Exception)
      {
        return false;
      }
    }

    /// <summary>
    /// Check if a user account is associeted to the given token
    /// </summary>
    /// <param name="token">The user license token</param>
    /// <returns>Pair of status code and HTTP resonse message. Status code is true if the user account is active. 
    /// Otherwise status code is false</returns>
    protected async Task<KeyValuePair<bool, HttpResponseMessage>> CheckUserAccountAsync(string token)
    {
      if (ApiUtility.ApiClient == null)
        ApiUtility.InitializeApiClient();

      ApiUtility.ApiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
      HttpResponseMessage userTokenHttpResponse = await ApiUtility.ApiClient.GetAsync(WebApiUrl.licenseUrl);

      return new KeyValuePair<bool, HttpResponseMessage>(userTokenHttpResponse.IsSuccessStatusCode == true, userTokenHttpResponse);
    }
  }
}
