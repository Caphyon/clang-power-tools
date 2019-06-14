using System.Net.Http;
using System.Net.Http.Headers;

namespace ClangPowerTools
{
  public static class ApiHelper
  {
    public static HttpClient ApiClient { get; set; }

    public static void InitializeApiClient()
    {
      ApiClient = new HttpClient();
      ApiClient.DefaultRequestHeaders.Accept.Clear();
      ApiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }
  }
}
