using System.Net.Http;
using System.Net.Http.Headers;

namespace ClangPowerTools
{
  public static class ApiUtility
  {
    public static HttpClient ApiClient { get; set; }

    public static void InitializeApiClient()
    {
      if (ApiClient != null) return;

      ApiClient = new HttpClient();
      ApiClient.DefaultRequestHeaders.Accept.Clear();
      ApiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }
  }
}
