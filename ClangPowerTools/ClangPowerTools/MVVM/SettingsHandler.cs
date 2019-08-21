using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace ClangPowerTools
{
  public class SettingsHandler
  {
    public string SettingsPath
    {
      get
      {
        return settingsPath;
      }
      private set { }
    }

    private string settingsPath = string.Empty;
    private readonly string SettingsFileName = "settings.json";
    private readonly string GeneralConfigurationFileName = "GeneralConfiguration.config";
    private readonly string FormatConfigurationFileName = "FormatConfiguration.config";
    private readonly string TidyOptionsConfigurationFileName = "TidyOptionsConfiguration.config";
    private readonly string TidyPredefinedChecksConfigurationFileName = "TidyPredefinedChecksConfiguration.config";

    public SettingsHandler()
    {
      SettingsPathBuilder settingsPathBuilder = new SettingsPathBuilder();
      settingsPath = settingsPathBuilder.GetPath("");
    }

    public void SaveSettings(string path)
    {
      List<object> models = CreateModelsList();

      using (StreamWriter file = File.CreateText(GetSettingsFilePath(path, SettingsFileName)))
      {
        JsonSerializer serializer = new JsonSerializer();
        serializer.Formatting = Formatting.Indented;
        serializer.Serialize(file, models);
      }
    }

    public void LoadSettings(string path)
    {
      using (StreamReader sw = new StreamReader(GetSettingsFilePath(path, SettingsFileName)))
      {
        string json = sw.ReadToEnd();
        JsonSerializer serializer = new JsonSerializer();
        List<object> models = JsonConvert.DeserializeObject<List<object>>(json);

        //TODO handle error deserialization

        SettingsModelProvider.CompilerSettings = JsonConvert.DeserializeObject<CompilerSettingsModel>(models[0].ToString());
        SettingsModelProvider.FormatSettings = JsonConvert.DeserializeObject<FormatSettingsModel>(models[1].ToString());
        SettingsModelProvider.TidySettings = JsonConvert.DeserializeObject<TidySettingsModel>(models[2].ToString());
      }
    }

    public bool SettingsFileExists()
    {
      string path = GetSettingsFilePath(settingsPath, SettingsFileName);
      return File.Exists(path);
    }

    public bool OldGeneralSettingsExists()
    {
      string path = GetSettingsFilePath(settingsPath, GeneralConfigurationFileName);
      return File.Exists(path);
    }

    public void MapOldSettings()
    {
      ClangOptions clangOptions = LoadOldSettingsFromFile(new ClangOptions(), GeneralConfigurationFileName);
      MapClangOptionsToCompilerSettings(clangOptions);

      ClangFormatOptions clangFormatOptions = LoadOldSettingsFromFile(new ClangFormatOptions(), FormatConfigurationFileName);
      MapClangFormatOptionsToFormatSettings(clangFormatOptions);

      ClangTidyOptions clangTidyOptions = LoadOldSettingsFromFile(new ClangTidyOptions(), TidyOptionsConfigurationFileName);
      MapClangTidyOptionsToTidyettings(clangTidyOptions);

      ClangTidyPredefinedChecksOptions clangTidyPredefinedChecksOptions = LoadOldSettingsFromFile(new ClangTidyPredefinedChecksOptions(), TidyPredefinedChecksConfigurationFileName);
      MapTidyPredefinedChecksToTidyettings(clangTidyPredefinedChecksOptions);
    }

    private T LoadOldSettingsFromFile<T>(T settings, string settingsFileName) where T : new()
    {
      string path = GetSettingsFilePath(settingsPath, settingsFileName);

      if (File.Exists(path))
      {
        SerializeSettings(path, ref settings);
      }
      return settings;
    }

    public void DeleteOldSettings()
    {
      string[] files = Directory.GetFiles(settingsPath, "*.config");
      foreach (var file in files)
      {
        File.Delete(file);
      }
    }

    public void DeleteSettings()
    {
      string[] file = Directory.GetFiles(settingsPath, SettingsFileName);
      if (file.Length > 0)
      {
        File.Delete(file[0]);
      }
    }

    public void ResetSettings()
    {
      SettingsModelProvider.CompilerSettings = new CompilerSettingsModel();
      SettingsModelProvider.FormatSettings = new FormatSettingsModel();
    }

    private static List<object> CreateModelsList()
    {
      List<object> models = new List<object>();
      models.Add(SettingsModelProvider.CompilerSettings);
      models.Add(SettingsModelProvider.FormatSettings);
      models.Add(SettingsModelProvider.TidySettings);
      return models;
    }

    private string GetSettingsFilePath(string path, string fileName)
    {
      return Path.Combine(path, fileName);
    }

    private void SerializeSettings<T>(string path, ref T config) where T : new()
    {
      XmlSerializer serializer = new XmlSerializer();
      config = serializer.DeserializeFromFile<T>(path);
    }

    private void MapClangOptionsToCompilerSettings(ClangOptions clangOptions)
    {
      SettingsModelProvider.CompilerSettings.CompileFlags = clangOptions.ClangFlagsCollection;
      SettingsModelProvider.CompilerSettings.FilesToIgnore = clangOptions.FilesToIgnore;
      SettingsModelProvider.CompilerSettings.ProjectsToIgnore = clangOptions.ProjectsToIgnore;
      SettingsModelProvider.CompilerSettings.AdditionalIncludes = clangOptions.AdditionalIncludes;
      SettingsModelProvider.CompilerSettings.WarningsAsErrors = clangOptions.TreatWarningsAsErrors;
      SettingsModelProvider.CompilerSettings.ContinueOnError = clangOptions.Continue;
      SettingsModelProvider.CompilerSettings.ClangCompileAfterMSCVCompile = clangOptions.ClangCompileAfterVsCompile;
      SettingsModelProvider.CompilerSettings.VerboseMode = clangOptions.VerboseMode;
      SettingsModelProvider.CompilerSettings.Version = clangOptions.Version;
    }

    private void MapClangFormatOptionsToFormatSettings(ClangFormatOptions clangFormat)
    {
      SettingsModelProvider.FormatSettings.FileExtensions = clangFormat.FileExtensions;
      SettingsModelProvider.FormatSettings.FilesToIgnore = clangFormat.SkipFiles;
      SettingsModelProvider.FormatSettings.AssumeFilename = clangFormat.AssumeFilename;
      SettingsModelProvider.FormatSettings.CustomExecutable = clangFormat.ClangFormatPath.Value;
      SettingsModelProvider.FormatSettings.Style = clangFormat.Style;
      SettingsModelProvider.FormatSettings.FallbackStyle = clangFormat.FallbackStyle;
      SettingsModelProvider.FormatSettings.FormatOnSave = clangFormat.EnableFormatOnSave;
    }

    private void MapClangTidyOptionsToTidyettings(ClangTidyOptions clangTidy)
    {
      SettingsModelProvider.TidySettings.HeaderFilter = clangTidy.HeaderFilter;
      SettingsModelProvider.TidySettings.Checks = clangTidy.TidyChecksCollection;
      SettingsModelProvider.TidySettings.CustomExecutable = clangTidy.ClangTidyPath.Value;
      SettingsModelProvider.TidySettings.FormatAfterTidy = clangTidy.FormatAfterTidy;
      SettingsModelProvider.TidySettings.TidyOnSave = clangTidy.AutoTidyOnSave;
    }

    private void MapTidyPredefinedChecksToTidyettings(ClangTidyPredefinedChecksOptions clangTidyPredefinedChecksOptions)
    {
      PropertyInfo[] properties = typeof(ClangTidyPredefinedChecksOptions).GetProperties();

      foreach (PropertyInfo propertyInfo in properties)
      {
        bool isChecked = (bool)propertyInfo.GetValue(new ClangTidyPredefinedChecksOptions(), null);

        if (isChecked)
        {
          SettingsModelProvider.TidySettings.Checks += string.Concat(FormatTidyCheckName(propertyInfo.Name), ";");
        }
      }
    }

    private string FormatTidyCheckName(string name)
    {
      StringBuilder stringBuilder = new StringBuilder();

      stringBuilder.Append(name[0]);
      for (int i = 1; i < name.Length; i++)
      {
        if (Char.IsUpper(name[i]))
        {
          stringBuilder.Append(name[i]).Append("-");
        }
        stringBuilder.Append(name[i]);
      }

      return stringBuilder.ToString().ToLower();
    }
  }
}
