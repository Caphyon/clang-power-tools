using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ClangPowerTools.Account
{
  public class NetworkUtility
  {
    public static async Task CheckInternetConnectionAsync()
    {
      try
      {
        using (HttpResponseMessage result = await ApiHelper.ApiClient.GetAsync("http://www.google.com"))
        {

        }
      }
      catch (Exception)
      {
        throw;
      }
    }
  }
}
