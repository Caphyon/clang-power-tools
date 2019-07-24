using System;
using System.IO;
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

    public static void CopySettingsProperties<T>(T target, T source)
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

    public static void SaveToFile(string aFilePath, object config)
    {
      XmlSerializer serializer = new XmlSerializer();
      serializer.SerializeToFile(aFilePath, config);
    }

    public static TSettings LoadFromFile<TSettings>(string aFilePath, TSettings config) where TSettings : new()
    {
      XmlSerializer serializer = new XmlSerializer();

      config = File.Exists(aFilePath) ? serializer.DeserializeFromFile<TSettings>(aFilePath) : new TSettings();
      return config;
    }
  }
}
