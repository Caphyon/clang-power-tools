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

    /// <summary>
    /// Save settings at a custom path
    /// </summary>
    /// <param name="path"></param>
    public void SaveSettings(string path)
    {
      List<object> models = CreateModelsList();
      SerializeSettings(models, path);
    }

    /// <summary>
    /// Save settings at the predefined path
    /// </summary>
    public void SaveSettings()
    {
      List<object> models = CreateModelsList();
      string path = GetSettingsFilePath(settingsPath, SettingsFileName);
      SerializeSettings(models, path);
    }

    /// <summary>
    /// Load settings from a custom path
    /// </summary>
    /// <param name="path"></param>
    public void LoadSettings(string path)
    {
      DeserializeSettings(path);
    }

    /// <summary>
    /// Load settings from the predefined path
    /// </summary>
    public void LoadSettings()
    {
      string path = GetSettingsFilePath(settingsPath, SettingsFileName);
      DeserializeSettings(path);
    }

    public bool SettingsFileExists()
    {
      string path = GetSettingsFilePath(settingsPath, SettingsFileName);
      return File.Exists(path);
    }

    public void ImportOldSettings()
    {
      if (OldGeneralSettingsExists())
      {
        MapOldSettings();
        SaveSettings();
        DeleteOldSettings();
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
      SettingsViewModelProvider.CompilerSettingsViewModel.CompilerModel = new CompilerSettingsModel();
      SettingsViewModelProvider.FormatSettingsViewModel.FormatModel = new FormatSettingsModel();
      SettingsViewModelProvider.TidySettingsViewModel.TidyModel = new TidySettingsModel();
      SaveSettings();
    }

    private bool OldGeneralSettingsExists()
    {
      string path = GetSettingsFilePath(settingsPath, GeneralConfigurationFileName);
      return File.Exists(path);
    }

    private void MapOldSettings()
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

    private void DeleteOldSettings()
    {
      string[] files = Directory.GetFiles(settingsPath, "*.config");
      foreach (var file in files)
      {
        File.Delete(file);
      }
    }

    private List<object> CreateModelsList()
    {
      List<object> models = new List<object>();
      models.Add(SettingsViewModelProvider.CompilerSettingsViewModel.CompilerModel);
      models.Add(SettingsViewModelProvider.FormatSettingsViewModel.FormatModel);
      models.Add(SettingsViewModelProvider.TidySettingsViewModel.TidyModel);
      return models;
    }

    private void SerializeSettings(List<object> models, string path)
    {
      using (StreamWriter file = File.CreateText(path))
      {
        JsonSerializer serializer = new JsonSerializer();
        serializer.Formatting = Formatting.Indented;
        serializer.Serialize(file, models);
      }
    }

    private void DeserializeSettings(string path)
    {
      using (StreamReader sw = new StreamReader(path))
      {
        string json = sw.ReadToEnd();
        JsonSerializer serializer = new JsonSerializer();
        List<object> models = JsonConvert.DeserializeObject<List<object>>(json);

        //TODO handle error deserialization

        SettingsViewModelProvider.CompilerSettingsViewModel.CompilerModel = JsonConvert.DeserializeObject<CompilerSettingsModel>(models[0].ToString());
        SettingsViewModelProvider.FormatSettingsViewModel.FormatModel = JsonConvert.DeserializeObject<FormatSettingsModel>(models[1].ToString());
        SettingsViewModelProvider.TidySettingsViewModel.TidyModel = JsonConvert.DeserializeObject<TidySettingsModel>(models[2].ToString());
      }
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
      CompilerSettingsModel compilerSettingsModel = new CompilerSettingsModel();
      compilerSettingsModel.CompileFlags = clangOptions.ClangFlagsCollection;
      compilerSettingsModel.FilesToIgnore = clangOptions.FilesToIgnore;
      compilerSettingsModel.ProjectsToIgnore = clangOptions.ProjectsToIgnore;
      compilerSettingsModel.AdditionalIncludes = clangOptions.AdditionalIncludes;
      compilerSettingsModel.WarningsAsErrors = clangOptions.TreatWarningsAsErrors;
      compilerSettingsModel.ContinueOnError = clangOptions.Continue;
      compilerSettingsModel.ClangCompileAfterMSCVCompile = clangOptions.ClangCompileAfterVsCompile;
      compilerSettingsModel.VerboseMode = clangOptions.VerboseMode;
      compilerSettingsModel.Version = clangOptions.Version;

      SettingsViewModelProvider.CompilerSettingsViewModel.CompilerModel = compilerSettingsModel;
    }

    private void MapClangFormatOptionsToFormatSettings(ClangFormatOptions clangFormat)
    {
      FormatSettingsModel formatSettingsModel = new FormatSettingsModel();
      formatSettingsModel.FileExtensions = clangFormat.FileExtensions;
      formatSettingsModel.FilesToIgnore = clangFormat.SkipFiles;
      formatSettingsModel.AssumeFilename = clangFormat.AssumeFilename;
      formatSettingsModel.CustomExecutable = clangFormat.ClangFormatPath.Value;
      formatSettingsModel.Style = clangFormat.Style;
      formatSettingsModel.FallbackStyle = clangFormat.FallbackStyle;
      formatSettingsModel.FormatOnSave = clangFormat.EnableFormatOnSave;

      SettingsViewModelProvider.FormatSettingsViewModel.FormatModel = formatSettingsModel;
    }

    private void MapClangTidyOptionsToTidyettings(ClangTidyOptions clangTidy)
    {
      TidySettingsModel tidySettingsModel = new TidySettingsModel();
      tidySettingsModel.HeaderFilter = clangTidy.HeaderFilter;
      tidySettingsModel.Checks = clangTidy.TidyChecksCollection;
      tidySettingsModel.CustomExecutable = clangTidy.ClangTidyPath.Value;
      tidySettingsModel.FormatAfterTidy = clangTidy.FormatAfterTidy;
      tidySettingsModel.TidyOnSave = clangTidy.AutoTidyOnSave;

      SettingsViewModelProvider.TidySettingsViewModel.TidyModel = tidySettingsModel;
    }

    private void MapTidyPredefinedChecksToTidyettings(ClangTidyPredefinedChecksOptions clangTidyPredefinedChecksOptions)
    {
      PropertyInfo[] properties = typeof(ClangTidyPredefinedChecksOptions).GetProperties();

      foreach (PropertyInfo propertyInfo in properties)
      {
        bool isChecked = (bool)propertyInfo.GetValue(new ClangTidyPredefinedChecksOptions(), null);

        if (isChecked)
        {
          SettingsViewModelProvider.TidySettingsViewModel.TidyModel.Checks += string.Concat(FormatTidyCheckName(propertyInfo.Name), ";");
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
