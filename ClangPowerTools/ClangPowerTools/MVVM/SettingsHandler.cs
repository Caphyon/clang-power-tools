using ClangPowerTools.Helpers;
using ClangPowerTools.MVVM.Controllers;
using ClangPowerTools.MVVM.LicenseValidation;
using ClangPowerTools.MVVM.Models;
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
    #region Members
    public static Action RefreshSettingsView;

    private readonly string settingsPath = string.Empty;
    private const string SettingsFileName = "settings.json";
    private const string UserProfileFileName = "userProfile.json";
    private const string GeneralConfigurationFileName = "GeneralConfiguration.config";
    private const string FormatConfigurationFileName = "FormatConfiguration.config";
    private const string TidyOptionsConfigurationFileName = "TidyOptionsConfiguration.config";
    private const string TidyPredefinedChecksConfigurationFileName = "TidyPredefinedChecksConfiguration.config";
    private const int MinJsonElements = 5;
    #endregion

    #region Constructor
    public SettingsHandler()
    {
      var settingsPathBuilder = new SettingsPathBuilder();
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

    public async Task InitializeAccountSettingsAsync()
    {
      SettingsProvider.AccountModel = new AccountModel();
      AccountModel loadedAccountModel = null;

      var networkConnected = await NetworkUtility.CheckInternetConnectionAsync();

      if (networkConnected)
      {
        loadedAccountModel = await LoadServerAccountSettingsAsync();
      }
      else if (!networkConnected || loadedAccountModel == null)
      {
        loadedAccountModel = LoadLocalAccountSettings();
      }

      if (loadedAccountModel == null)
        return;

      SettingsProvider.AccountModel = new AccountModel
      {
        UserName = loadedAccountModel.UserName,
        Email = loadedAccountModel.Email,
        LicenseType = loadedAccountModel.LicenseType,
        LicenseExpirationDate = loadedAccountModel.LicenseExpirationDate,
        Version = loadedAccountModel.Version
      };

      RefreshSettingsView?.Invoke();
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
      if (File.Exists(path))
      {
        string json = ReadSettingsFile(path);
        DeserializeSettings(json);
        RefreshSettingsView?.Invoke();
      }
    }

    /// <summary>
    /// Load settings from the predefined path
    /// </summary>
    public void LoadSettings()
    {
      string path = GetSettingsFilePath(settingsPath, SettingsFileName);
      if (File.Exists(path))
      {
        string json = ReadSettingsFile(path);
        DeserializeSettings(json);
      }
      else
      {
        CreateDeaultSettings();
      }
    }

    public bool SettingsFileExists()
    {
      string path = GetSettingsFilePath(settingsPath, SettingsFileName);
      return File.Exists(path);
    }

    public void ResetSettings()
    {
      CreateDeaultSettings();
      RefreshSettingsView?.Invoke();
    }

    public string GetSettingsAsJson()
    {
      List<object> models = CreateModelsList();
      return JsonConvert.SerializeObject(models);
    }

    public void LoadCloudSettings(string json)
    {
      DeserializeSettings(json);
      SaveSettings();
      RefreshSettingsView?.Invoke();
    }


    private AccoutApiModel DeserializeUserAccountDetails(string json)
    {
      var accoutApiModel = JsonConvert.DeserializeObject<AccoutApiModel>(json);
      return accoutApiModel;
    }

    private LicenseModel DeserializeLicenseDetails(string json)
    {
      var userLicenseCollection = JsonConvert.DeserializeObject<List<LicenseModel>>(json);
      return userLicenseCollection[0];
    }

    #endregion


    #region Private Methods

    private void CreateDeaultSettings()
    {
      SettingsProvider.CompilerSettingsModel = new CompilerSettingsModel();
      SettingsProvider.FormatSettingsModel = new FormatSettingsModel();
      SettingsProvider.TidySettingsModel = new TidySettingsModel();
      SettingsProvider.LlvmSettingsModel = new LlvmSettingsModel();

      SetDefaultTidyPredefindedChecks();
    }

    private void SetDefaultTidyPredefindedChecks()
    {
      var checks = new StringBuilder();
      var tidyCheckModel = new TidyChecks();
      var tidyModel = SettingsProvider.TidySettingsModel;

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
        SettingsProvider.TidySettingsModel,
        SettingsProvider.GeneralSettingsModel,
        SettingsProvider.LlvmSettingsModel,
      };
      return models;
    }

    private void SerializeSettings(List<object> models, string path)
    {
      using StreamWriter file = File.CreateText(path);
      var serializer = new JsonSerializer
      {
        Formatting = Formatting.Indented
      };
      serializer.Serialize(file, models);
    }

    private void DeserializeSettings(string json)
    {
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

    private string ReadSettingsFile(string path)
    {
      using StreamReader sw = new StreamReader(path);
      return sw.ReadToEnd();
    }

    private void SetSettingsModels(CompilerSettingsModel compilerModel, FormatSettingsModel formatModel,
      TidySettingsModel tidyModel, GeneralSettingsModel generalModel, LlvmSettingsModel llvmModel)
    {
      SettingsProvider.CompilerSettingsModel = compilerModel;
      SettingsProvider.FormatSettingsModel = formatModel;
      SettingsProvider.TidySettingsModel = tidyModel;
      SettingsProvider.GeneralSettingsModel = generalModel;
      SettingsProvider.LlvmSettingsModel = llvmModel;
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
      var accountModel = new AccountModel();

      compilerSettingsModel.CompileFlags = clangOptions.ClangFlagsCollection;
      compilerSettingsModel.FilesToIgnore = clangOptions.FilesToIgnore;
      compilerSettingsModel.ProjectsToIgnore = clangOptions.ProjectsToIgnore;
      compilerSettingsModel.WarningsAsErrors = clangOptions.TreatWarningsAsErrors;
      compilerSettingsModel.ContinueOnError = clangOptions.Continue;
      compilerSettingsModel.ClangAfterMSVC = clangOptions.ClangCompileAfterVsCompile;
      compilerSettingsModel.VerboseMode = clangOptions.VerboseMode;
      accountModel.Version = clangOptions.Version;

      SettingsProvider.CompilerSettingsModel = compilerSettingsModel;
      SettingsProvider.AccountModel = accountModel;
    }

    private void MapClangFormatOptionsToSettings(ClangFormatOptions clangFormat)
    {
      var formatSettingsModel = new FormatSettingsModel
      {
        FileExtensions = clangFormat.FileExtensions,
        FilesToIgnore = clangFormat.SkipFiles,
        AssumeFilename = clangFormat.AssumeFilename,
        CustomExecutable = clangFormat.ClangFormatPath.Value,
        Style = clangFormat.Style,
        FallbackStyle = clangFormat.FallbackStyle,
        FormatOnSave = clangFormat.EnableFormatOnSave
      };

      SettingsProvider.FormatSettingsModel = formatSettingsModel;
    }

    private void MapClangTidyOptionsToSettings(ClangTidyOptions clangTidy)
    {
      var tidySettingsModel = new TidySettingsModel
      {
        HeaderFilter = clangTidy.HeaderFilter,
        CustomChecks = clangTidy.TidyChecksCollection,
        CustomExecutable = clangTidy.ClangTidyPath.Value,
        FormatAfterTidy = clangTidy.FormatAfterTidy,
        TidyOnSave = clangTidy.AutoTidyOnSave
      };

      SettingsProvider.TidySettingsModel = tidySettingsModel;
    }

    private void MapTidyPredefinedChecksToTidyettings(ClangTidyPredefinedChecksOptions clangTidyPredefinedChecksOptions)
    {
      PropertyInfo[] properties = typeof(ClangTidyPredefinedChecksOptions).GetProperties();
      var tidySettingsModel = SettingsProvider.TidySettingsModel;

      foreach (PropertyInfo propertyInfo in properties)
      {
        bool isChecked = (bool)propertyInfo.GetValue(clangTidyPredefinedChecksOptions, null);

        if (isChecked)
        {
          tidySettingsModel.PredefinedChecks += string.Concat(FormatTidyCheckName(propertyInfo.Name), ";");
        }
      }

      SettingsProvider.TidySettingsModel = tidySettingsModel;
    }

    private string FormatTidyCheckName(string name)
    {
      var stringBuilder = new StringBuilder();
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

    /// <summary>
    /// Load the user profile data from the local file
    /// </summary>
    /// <returns>The loaded user profile model</returns>
    private AccountModel LoadLocalAccountSettings()
    {
      var path = Path.Combine(settingsPath, UserProfileFileName);
      if (!File.Exists(path))
        return null;

      var json = ReadSettingsFile(path);
      var accountModel = JsonConvert.DeserializeObject<AccountModel>(json);

      return accountModel;
    }

    /// <summary>
    /// Load the user profile data from the server
    /// </summary>
    /// <returns>The loaded user profile model</returns>
    private async Task<AccountModel> LoadServerAccountSettingsAsync()
    {
      var settingsApi = new SettingsApi();

      // User profile
      var accountDetailsJson = await settingsApi.GetUserAccountProfileJsonAsync();

      if (string.IsNullOrWhiteSpace(accountDetailsJson))
        return null;

      var accountApiModel = DeserializeUserAccountDetails(accountDetailsJson);
      if (accountApiModel == null)
        return null;

      // License type
      LicenseType licenseType = await new LicenseController().GetUserLicenseTypeAsync();

      // License expiration date
      var expirationDate = string.Empty;
      var licenseDetailsJson = await settingsApi.GetLicenseDetailsJsonAsync();
      if (!string.IsNullOrWhiteSpace(licenseDetailsJson))
      {
        expirationDate = !string.IsNullOrWhiteSpace(licenseDetailsJson) ?
        DeserializeLicenseDetails(licenseDetailsJson).expires : string.Empty;
      }

      // Version from file
      var localAccountModel = LoadLocalAccountSettings();

      // Create the complete Account model object
      var accountModel = new AccountModel
      {
        UserName = $"{accountApiModel.firstname} {accountApiModel.lastname}",
        Email = accountApiModel.email,
        LicenseType = licenseType,

        // TODO : throw exception if the string param cannot be converted to a valid date
        LicenseExpirationDate = DateTime.Parse(expirationDate),

        Version = localAccountModel == null ? string.Empty : localAccountModel.Version
      };

      return accountModel;
    }

    #endregion

  }
}
