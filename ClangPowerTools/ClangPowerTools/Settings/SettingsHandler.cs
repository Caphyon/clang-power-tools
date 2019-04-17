using System;
using System.Reflection;

namespace ClangPowerTools
{
  public class SettingsHandler : SettingsProvider
  {
    public static void SaveAll()
    {
      GeneralSettings.SaveSettingsToStorage();
      TidySettings.SaveSettingsToStorage();
      TidyCustomCheckes.SaveSettingsToStorage();
      TidyPredefinedChecks.SaveSettingsToStorage();
      ClangFormatSettings.SaveSettingsToStorage();
    }

    public static void SaveGeneralSettings() => GeneralSettings.SaveSettingsToStorage();

    public static void SaveTidySettings() => TidySettings.SaveSettingsToStorage();

    public static void SaveTidyCustomChecksSettings() => TidyCustomCheckes.SaveSettingsToStorage();

    public static void SaveTidyPredefinedChecksSettings() => TidyPredefinedChecks.SaveSettingsToStorage();

    public static void SaveFormatSettings() => ClangFormatSettings.SaveSettingsToStorage();

    public static void CopySettingsProperties<T>(T source, T target)
    {
      Type type = source.GetType();
      foreach (PropertyInfo property in type.GetProperties())
      {
        if (property.CanWrite)
        {
          property.SetValue(target, property.GetValue(source, null), null);
        }
      }
    }
  }
}
