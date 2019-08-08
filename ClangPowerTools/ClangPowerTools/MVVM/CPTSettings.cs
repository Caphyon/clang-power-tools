using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace ClangPowerTools
{
  public class CPTSettings
  {
    private string settingsPath = string.Empty;
    private readonly string SettingsFileName = "settings.json";
    private readonly string GeneralConfigurationFileName = "GeneralConfiguration.config";
    private readonly string FormatConfigurationFileName = "FormatConfiguration.config";
    private readonly string TidyOptionsConfigurationFileName = "TidyOptionsConfiguration.config";
    private readonly string TidyPredefinedChecksConfigurationFileName = "TidyPredefinedChecksConfiguration.config";

    public CPTSettings()
    {
      SettingsPathBuilder settingsPathBuilder = new SettingsPathBuilder();
      settingsPath = settingsPathBuilder.GetPath("");
    }


    public void SerializeSettings()
    {
      List<object> models = CreateModelsList();

      using (StreamWriter file = File.CreateText(GetSettingsFilePath(settingsPath, SettingsFileName)))
      {
        JsonSerializer serializer = new JsonSerializer();
        serializer.Formatting = Formatting.Indented;
        serializer.Serialize(file, models);
      }
    }

    public void DeserializeSettings()
    {
      using (StreamReader sw = new StreamReader(GetSettingsFilePath(settingsPath, SettingsFileName)))
      {
        string json = sw.ReadToEnd();
        JsonSerializer serializer = new JsonSerializer();
        List<object> models = JsonConvert.DeserializeObject<List<object>>(json);

        //TODO handle error deserialization

        SettingsModelHandler.CompilerSettings = JsonConvert.DeserializeObject<CompilerSettingsModel>(models[0].ToString());
        SettingsModelHandler.FormatSettings = JsonConvert.DeserializeObject<FormatSettingsModel>(models[1].ToString());
        SettingsModelHandler.TidySettings = JsonConvert.DeserializeObject<TidySettingsModel>(models[2].ToString());
      }
    }

    private void CheckOldSettings()
    {
      ClangOptions clangOptions = LoadOldSettingsFromFile(new ClangOptions(), GeneralConfigurationFileName);
      MapClangOptionsToCompilerSettings(clangOptions);

      ClangFormatOptions clangFormatOptions = LoadOldSettingsFromFile(new ClangFormatOptions(), FormatConfigurationFileName);
      MapClangFormatOptionsToFormatSettings(clangFormatOptions);

      ClangTidyOptions clangTidyOptions = LoadOldSettingsFromFile(new ClangTidyOptions(), TidyOptionsConfigurationFileName);
      MapClangTidyOptionsToTidyettings(clangTidyOptions);


      SerializeSettings();
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

    private void DeleteSettings()
    {
      string[] file = Directory.GetFiles(settingsPath, SettingsFileName);
      if (file.Length > 0)
      {
        File.Delete(file[0]);
      }
    }

    public void ResetSettings()
    {
      SettingsModelHandler.CompilerSettings = new CompilerSettingsModel();
      SettingsModelHandler.FormatSettings = new FormatSettingsModel();
    }

    private static List<object> CreateModelsList()
    {
      List<object> models = new List<object>();
      models.Add(SettingsModelHandler.CompilerSettings);
      models.Add(SettingsModelHandler.FormatSettings);
      models.Add(SettingsModelHandler.TidySettings);
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
      SettingsModelHandler.CompilerSettings.CompileFlags = clangOptions.ClangFlagsCollection;
      SettingsModelHandler.CompilerSettings.FilesToIgnore = clangOptions.FilesToIgnore;
      SettingsModelHandler.CompilerSettings.ProjectsToIgnore = clangOptions.ProjectsToIgnore;
      SettingsModelHandler.CompilerSettings.AdditionalIncludes = clangOptions.AdditionalIncludes;
      SettingsModelHandler.CompilerSettings.WarningsAsErrors = clangOptions.TreatWarningsAsErrors;
      SettingsModelHandler.CompilerSettings.ContinueOnError = clangOptions.Continue;
      SettingsModelHandler.CompilerSettings.ClangCompileAfterMSCVCompile = clangOptions.ClangCompileAfterVsCompile;
      SettingsModelHandler.CompilerSettings.VerboseMode = clangOptions.VerboseMode;
      SettingsModelHandler.CompilerSettings.Version = clangOptions.Version;
    }

    private void MapClangFormatOptionsToFormatSettings(ClangFormatOptions clangFormat)
    {
      SettingsModelHandler.FormatSettings.FileExtensions = clangFormat.FileExtensions;
      SettingsModelHandler.FormatSettings.FilesToIgnore = clangFormat.SkipFiles;
      SettingsModelHandler.FormatSettings.AssumeFilename = clangFormat.AssumeFilename;
      SettingsModelHandler.FormatSettings.CustomExecutable = clangFormat.ClangFormatPath.Value;
      SettingsModelHandler.FormatSettings.Style = clangFormat.Style;
      SettingsModelHandler.FormatSettings.FallbackStyle = clangFormat.FallbackStyle;
      SettingsModelHandler.FormatSettings.FormatOnSave = clangFormat.EnableFormatOnSave;
    }

    private void MapClangTidyOptionsToTidyettings(ClangTidyOptions clangTidy)
    {
      SettingsModelHandler.TidySettings.HeaderFilter = clangTidy.HeaderFilter;
      SettingsModelHandler.TidySettings.UseChecksFrom = clangTidy.TidyMode;
      SettingsModelHandler.TidySettings.CustomChecks = clangTidy.TidyChecksCollection;
      SettingsModelHandler.TidySettings.CustomExecutable = clangTidy.ClangTidyPath.Value;
      SettingsModelHandler.TidySettings.FormatAfterTidy = clangTidy.FormatAfterTidy;
      SettingsModelHandler.TidySettings.TidyOnSave = clangTidy.AutoTidyOnSave;
    }

    private void MapClangTidyPredefinedChecksOptionsToTidyettings(ClangTidyPredefinedChecksOptions clangTidy)
    {


    }
  }
}
