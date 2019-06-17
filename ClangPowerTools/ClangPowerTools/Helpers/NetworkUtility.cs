using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ClangPowerTools.Helpers
{
  public class NetworkUtility
  {
    public static async Task<bool> CheckInternetConnectionAsync()
    {
      try
      {
        using (HttpResponseMessage result = await ApiUtility.ApiClient.GetAsync("http://www.google.com"))
        {
          return result != null;
        }
      }
      catch (Exception)
      {
        return false;
      }
    }
  }
}
