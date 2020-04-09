using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace ClangPowerTools
{
  public class SettingsHandler
  {
    private readonly string settingsPath = string.Empty;
    private readonly SettingsProvider settingsProvider = new SettingsProvider();

    private const string SettingsFileName = "settings.json";
    private const string GeneralConfigurationFileName = "GeneralConfiguration.config";
    private const string FormatConfigurationFileName = "FormatConfiguration.config";
    private const string TidyOptionsConfigurationFileName = "TidyOptionsConfiguration.config";
    private const string TidyPredefinedChecksConfigurationFileName = "TidyPredefinedChecksConfiguration.config";
    private const int MinJsonElements =  5 ;

    #region Constructor
    public SettingsHandler()
    {
      SettingsPathBuilder settingsPathBuilder = new SettingsPathBuilder();
      settingsPath = settingsPathBuilder.GetPath("");
    }
    #endregion

    #region Public Methods 
    /// <summary>
    /// Load settings or import old settings
    /// </summary>
    public void InitializeSettings()
    {
      if (SettingsFileExists())
      {
        LoadSettings();
      }
      else if (OldGeneralSettingsExists())
      {
        ImportOldSettings();
      }
      else
      {
        CreateDeaultSettings();
      }
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
      CreateDeaultSettings();
    }

    #endregion

    #region Private Methods

    private void CreateDeaultSettings()
    {
      SettingsProvider.CompilerSettingsModel = new CompilerSettingsModel();
      SettingsProvider.FormatSettingsModel = new FormatSettingsModel();
      settingsProvider.SetTidySettingsModel(new TidySettingsModel());
      settingsProvider.SetLlvmSettingsModel(new LlvmSettingsModel());

      SetDefaultTidyPredefindedChecks();
    }

    private void SetDefaultTidyPredefindedChecks()
    {
      var checks = new StringBuilder();
      var tidyCheckModel = new TidyChecks();
      var tidyModel = settingsProvider.GetTidySettingsModel();

      foreach (TidyCheckModel item in tidyCheckModel.Checks)
      {
        if (item.IsChecked)
        {
          checks.Append(item.Name).Append(";");
        }
      }
      checks.Length--;
      tidyModel.PredefinedChecks = checks.ToString();
    }

    private void ImportOldSettings()
    {
      MapOldSettings();
      SaveSettings();
      DeleteOldSettings();
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
      List<object> models = new List<object>
      {
        SettingsProvider.CompilerSettingsModel,
        SettingsProvider.FormatSettingsModel,
        settingsProvider.GetTidySettingsModel(),
        settingsProvider.GetGeneralSettingsModel(),
        settingsProvider.GetLlvmSettingsModel(),
      };
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
        var serializer = new JsonSerializer();

        try
        {
          var models = JsonConvert.DeserializeObject<List<object>>(json);
          var compilerModel = JsonConvert.DeserializeObject<CompilerSettingsModel>(models[0].ToString());
          var formatModel = JsonConvert.DeserializeObject<FormatSettingsModel>(models[1].ToString());
          var tidyModel = JsonConvert.DeserializeObject<TidySettingsModel>(models[2].ToString());
          var generalModel = JsonConvert.DeserializeObject<GeneralSettingsModel>(models[3].ToString());

          LlvmSettingsModel llvmModel;
          if (models.Count >= MinJsonElements)
          {
            llvmModel = JsonConvert.DeserializeObject<LlvmSettingsModel>(models[4].ToString());
          }
          else
          {
            llvmModel = new LlvmSettingsModel();
          }

          SetSettingsModels(compilerModel, formatModel, tidyModel, generalModel, llvmModel);
        }
        catch (Exception e)
        {
          MessageBox.Show(e.Message, "Cannot Load Clang Power Tools Settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
      }
    }

    private void SetSettingsModels(CompilerSettingsModel compilerModel, FormatSettingsModel formatModel, TidySettingsModel tidyModel, GeneralSettingsModel generalModel, LlvmSettingsModel llvmModel)
    {
      SettingsProvider.CompilerSettingsModel = compilerModel;
      SettingsProvider.FormatSettingsModel = formatModel;
      settingsProvider.SetTidySettingsModel(tidyModel);
      settingsProvider.SetGeneralSettingsModel(generalModel);
      settingsProvider.SetLlvmSettingsModel(llvmModel);
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
      var compilerSettingsModel = new CompilerSettingsModel();
      var generalSettingsModel = new GeneralSettingsModel();

      compilerSettingsModel.CompileFlags = clangOptions.ClangFlagsCollection;
      compilerSettingsModel.FilesToIgnore = clangOptions.FilesToIgnore;
      compilerSettingsModel.ProjectsToIgnore = clangOptions.ProjectsToIgnore;
      compilerSettingsModel.WarningsAsErrors = clangOptions.TreatWarningsAsErrors;
      compilerSettingsModel.ContinueOnError = clangOptions.Continue;
      compilerSettingsModel.ClangAfterMSVC = clangOptions.ClangCompileAfterVsCompile;
      compilerSettingsModel.VerboseMode = clangOptions.VerboseMode;
      generalSettingsModel.Version = clangOptions.Version;


      SettingsProvider.CompilerSettingsModel = compilerSettingsModel;
      settingsProvider.SetGeneralSettingsModel(generalSettingsModel);
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

      SettingsProvider.FormatSettingsModel = formatSettingsModel;
    }

    private void MapClangTidyOptionsToSettings(ClangTidyOptions clangTidy)
    {
      TidySettingsModel tidySettingsModel = new TidySettingsModel();
      tidySettingsModel.HeaderFilter = clangTidy.HeaderFilter;
      tidySettingsModel.CustomChecks = clangTidy.TidyChecksCollection;
      tidySettingsModel.CustomExecutable = clangTidy.ClangTidyPath.Value;
      tidySettingsModel.FormatAfterTidy = clangTidy.FormatAfterTidy;
      tidySettingsModel.TidyOnSave = clangTidy.AutoTidyOnSave;

      settingsProvider.SetTidySettingsModel(tidySettingsModel);
    }

    private void MapTidyPredefinedChecksToTidyettings(ClangTidyPredefinedChecksOptions clangTidyPredefinedChecksOptions)
    {
      PropertyInfo[] properties = typeof(ClangTidyPredefinedChecksOptions).GetProperties();
      TidySettingsModel tidySettingsModel = settingsProvider.GetTidySettingsModel();

      foreach (PropertyInfo propertyInfo in properties)
      {
        bool isChecked = (bool)propertyInfo.GetValue(clangTidyPredefinedChecksOptions, null);

        if (isChecked)
        {
          tidySettingsModel.PredefinedChecks += string.Concat(FormatTidyCheckName(propertyInfo.Name), ";");
        }
      }

      settingsProvider.SetTidySettingsModel(tidySettingsModel);
    }

    private string FormatTidyCheckName(string name)
    {
      StringBuilder stringBuilder = new StringBuilder();

      stringBuilder.Append(name[0]);
      for (int i = 1; i < name.Length; i++)
      {
        if (Char.IsUpper(name[i]))
        {
          stringBuilder.Append("-").Append(name[i]);
        }
        else
        {
          stringBuilder.Append(name[i]);
        }
      }

      return stringBuilder.ToString().ToLower();
    }
    #endregion

  }
}
