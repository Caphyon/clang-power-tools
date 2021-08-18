using Newtonsoft.Json;

namespace ClangPowerTools.Extensions
{
  public static class ObjectExtension
  {
    public static T Clone<T>(this T obj)
    {
      return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj));
    }
  }
}
