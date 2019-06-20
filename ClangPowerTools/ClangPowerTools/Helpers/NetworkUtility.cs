using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ClangPowerTools.Helpers
{
  public class NetworkUtility
  {
    public static async Task<bool> CheckInternetConnectionAsync()
    {
      CheckApiClient();

      try
      {
        using (HttpResponseMessage result = await ApiUtility.ApiClient.GetAsync("https://www.google.com"))
        {
          return result != null;
        }
      }
      catch (Exception)
      {
        return false;
      }
    }

    private static void CheckApiClient()
    {
      if (ApiUtility.ApiClient == null)
      {
        ApiUtility.InitializeApiClient();
      }
    }
  }
}
