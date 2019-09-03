using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
    /// Load settings or import old settings
    /// </summary>
    public void InitializeSettings()
    {
      _ = Task.Run(() =>
       {
         if (SettingsFileExists())
         {
           LoadSettings();
         }
         else
         {
           ImportOldSettings();
         }
       });
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

    public void ResetSettings()
    {
      SettingsViewModelProvider.CompilerSettingsViewModel.CompilerModel = new CompilerSettingsModel();
      SettingsViewModelProvider.FormatSettingsViewModel.FormatModel = new FormatSettingsModel();
      SettingsViewModelProvider.TidySettingsViewModel.TidyModel = new TidySettingsModel();
      SaveSettings();
    }

    private void ImportOldSettings()
    {
      if (OldGeneralSettingsExists())
      {
        MapOldSettings();
        SaveSettings();
        DeleteOldSettings();
      }
    }

    private bool OldGeneralSettingsExists()
    {
      string path = GetSettingsFilePath(settingsPath, GeneralConfigurationFileName);
      return File.Exists(path);
    }

    private void MapOldSettings()
    {
      ClangOptions clangOptions = LoadOldSettingsFromFile(new ClangOptions(), GeneralConfigurationFileName);
      MapClangOptionsToSettings(clangOptions);

      ClangFormatOptions clangFormatOptions = LoadOldSettingsFromFile(new ClangFormatOptions(), FormatConfigurationFileName);
      MapClangFormatOptionsToSettings(clangFormatOptions);

      ClangTidyOptions clangTidyOptions = LoadOldSettingsFromFile(new ClangTidyOptions(), TidyOptionsConfigurationFileName);
      MapClangTidyOptionsToSettings(clangTidyOptions);

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
      models.Add(SettingsViewModelProvider.GeneralSettingsViewModel.GeneralSettingsModel);
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

        try
        {
          List<object> models = JsonConvert.DeserializeObject<List<object>>(json);
          SettingsViewModelProvider.CompilerSettingsViewModel.CompilerModel = JsonConvert.DeserializeObject<CompilerSettingsModel>(models[0].ToString());
          SettingsViewModelProvider.FormatSettingsViewModel.FormatModel = JsonConvert.DeserializeObject<FormatSettingsModel>(models[1].ToString());
          SettingsViewModelProvider.TidySettingsViewModel.TidyModel = JsonConvert.DeserializeObject<TidySettingsModel>(models[2].ToString());
          SettingsViewModelProvider.GeneralSettingsViewModel.GeneralSettingsModel = JsonConvert.DeserializeObject<GeneralSettingsModel>(models[3].ToString());
        }
        catch (JsonException e)
        {
          MessageBox.Show(e.Message, "Cannot Load Clang Power Tools Settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
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

    private void MapClangOptionsToSettings(ClangOptions clangOptions)
    {
      CompilerSettingsModel compilerSettingsModel = new CompilerSettingsModel();
      GeneralSettingsModel generalSettingsModel = new GeneralSettingsModel();

      compilerSettingsModel.CompileFlags = clangOptions.ClangFlagsCollection;
      compilerSettingsModel.FilesToIgnore = clangOptions.FilesToIgnore;
      compilerSettingsModel.ProjectsToIgnore = clangOptions.ProjectsToIgnore;
      compilerSettingsModel.AdditionalIncludes = clangOptions.AdditionalIncludes;
      compilerSettingsModel.WarningsAsErrors = clangOptions.TreatWarningsAsErrors;
      compilerSettingsModel.ContinueOnError = clangOptions.Continue;
      compilerSettingsModel.ClangAfterMSVC = clangOptions.ClangCompileAfterVsCompile;
      compilerSettingsModel.VerboseMode = clangOptions.VerboseMode;
      generalSettingsModel.Version = clangOptions.Version;

      SettingsViewModelProvider.GeneralSettingsViewModel.GeneralSettingsModel = generalSettingsModel;
      SettingsViewModelProvider.CompilerSettingsViewModel.CompilerModel = compilerSettingsModel;
    }

    private void MapClangFormatOptionsToSettings(ClangFormatOptions clangFormat)
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

    private void MapClangTidyOptionsToSettings(ClangTidyOptions clangTidy)
    {
      TidySettingsModel tidySettingsModel = new TidySettingsModel();
      tidySettingsModel.HeaderFilter = clangTidy.HeaderFilter;
      tidySettingsModel.CustomChecks = clangTidy.TidyChecksCollection;
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
          SettingsViewModelProvider.TidySettingsViewModel.TidyModel.PredefinedChecks += string.Concat(FormatTidyCheckName(propertyInfo.Name), ";");
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
