using System;
using System.Reflection;

namespace ClangPowerTools.Helpers
{
  public static class PropertyHandler
  {
    /// <summary>
    /// Method get values of specified property of an object
    /// and the property names are available through PropertyInfo.Name property
    /// </summary>
    /// <typeparam name="T">template</typeparam>
    /// <param name="obj">the object that contains the property you want to get</param>
    /// <param name="propertyName">the name of the property you are trying to get</param>
    /// <returns></returns>
    public static object Get<T>(object obj, string propertyName) => typeof(T).GetProperty(propertyName)?.GetValue(obj);

    /// <summary>
    /// Method set values of specified property of an object
    /// </summary>
    /// <param name="obj">the object that contains the property you want to set</param>
    /// <param name="propertyName"></param>
    /// <param name="value">the name of the property you are trying to set</param>
    public static void Set(object obj, string propertyName, object value)
    {
      PropertyInfo propertyInfo = obj.GetType().GetProperty(propertyName);
      propertyInfo?.SetValue(obj, Convert.ChangeType(value, propertyInfo.PropertyType), null);
    }

  }
}
