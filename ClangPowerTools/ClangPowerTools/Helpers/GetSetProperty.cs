using System;
using System.Reflection;

namespace ClangPowerTools.Helpers
{
  public static class GetSetProperty
  {
    public static object Get<T>(object obj, string propertyName) => typeof(T).GetProperty(propertyName).GetValue(obj);

    public static void Set(object obj, string propertyName, object value)
    {
      PropertyInfo propertyInfo = obj.GetType().GetProperty(propertyName);
      propertyInfo.SetValue(obj, Convert.ChangeType(value, propertyInfo.PropertyType), null);
    }

  }
}
