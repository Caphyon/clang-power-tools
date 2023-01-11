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
      };
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

      string userProfilePath = GetSettingsFilePath(settingsPath, UserProfileFileName);
      SerializeSettings(SettingsProvider.AccountModel, userProfilePath);
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

    public async Task LicenseInfoUpdateAsync()
    {
      KeyValuePair<LicenseType, string> licenseInfo = await GetLicenseInfoAsync();

      SettingsProvider.AccountModel.LicenseType = licenseInfo.Key;

      if (licenseInfo.Key == LicenseType.Trial)
      {
        SettingsProvider.AccountModel.LicenseExpirationDate = GetTrialLicenseExpirationDate();
      }
      else
      {
        SettingsProvider.AccountModel.LicenseExpirationDate = !string.IsNullOrWhiteSpace(licenseInfo.Value) ?
          DateTime.Parse(licenseInfo.Value).ToString("MMMM dd yyyy") : "Never";
      }
    }

    public async Task UserProfileInfoUpdateAsync()
    {
      var accountModel = await GetUserProfileAsync();

      SettingsProvider.AccountModel.UserName = $"{accountModel.firstname} {accountModel.lastname}";
      SettingsProvider.AccountModel.Email = accountModel.email;
    }

    public string GetTrialLicenseExpirationDate()
    {
      var expirationDate = new FreeTrialController().GetExpirationDateAsString();

      return !string.IsNullOrWhiteSpace(expirationDate) ?
        DateTime.Parse(expirationDate).ToString("MMMM dd yyyy") : string.Empty;
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

      foreach (var item in TidyChecksDefault.Checks)
      {
          checks.Append(item).Append(";");
      }
      checks.Length--;
      SettingsProvider.TidySettingsModel.PredefinedChecks = checks.ToString();
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

    private void SerializeSettings(object models, string path)
    {
      // Remove the hidden attribute of the file in order to overwrite it
      FileInfo fileInfo;
      if (File.Exists(path))
      {
        fileInfo = new FileInfo(path);
        fileInfo.Attributes &= ~FileAttributes.Hidden;
      }

      // Overwrite the file
      using StreamWriter file = new StreamWriter(path);
      var serializer = new JsonSerializer
      {
        Formatting = Formatting.Indented
      };
      serializer.Serialize(file, models);

      // Set back the hidden attribute
      fileInfo = new FileInfo(path);
      fileInfo.Attributes |= FileAttributes.Hidden;
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
      SettingsProvider.GeneralSettingsModel = generalSettingsModel;
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

    /// <summary>
    /// Load the user profile data from the local file
    /// </summary>
    /// <returns>The loaded user profile model</returns>
    private AccountModel LoadLocalAccountSettings()
    {
      var path = GetSettingsFilePath(settingsPath, UserProfileFileName);
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
      // User profile
      var accountApiModel = await GetUserProfileAsync();

      // License
      KeyValuePair<LicenseType, string> licenseInfo = await GetLicenseInfoAsync();

      // Create the complete Account model object
      var accountModel = new AccountModel
      {
        UserName = $"{accountApiModel.firstname} {accountApiModel.lastname}",
        Email = accountApiModel.email,
        LicenseType = licenseInfo.Key,
        LicenseExpirationDate = !string.IsNullOrWhiteSpace(licenseInfo.Value) ?
          DateTime.Parse(licenseInfo.Value).ToString("MMMM dd yyyy") : "Never"
      };

      return accountModel;
    }

    /// <summary>
    /// Get the user profile information from the server
    /// </summary>
    /// <returns>User profile data as a model object</returns>
    private async Task<AccountApiModel> GetUserProfileAsync()
    {
      var settingsApi = new SettingsApi();
      var accountDetailsJson = await settingsApi.GetUserProfileJsonAsync();

      return !string.IsNullOrWhiteSpace(accountDetailsJson) ?
        DeserializeUserAccountDetails(accountDetailsJson) : new AccountApiModel();
    }

    /// <summary>
    /// Get the user license type and the expiration date
    /// </summary>
    /// <returns>A KeyValuePair with the license type as the key and license expiration date as the value</returns>
    private async Task<KeyValuePair<LicenseType, string>> GetLicenseInfoAsync()
    {
      // License type
      LicenseType licenseType = await new LicenseController().GetUserLicenseTypeAsync();

      // License expiration date
      var settingsApi = new SettingsApi();
      var licenseDetailsJson = await settingsApi.GetLicenseDetailsJsonAsync();
      // check for invalid return type after license request
      // check the length because personal license will return "[]" - empty json array 
      var expirationDate = !string.IsNullOrWhiteSpace(licenseDetailsJson) && licenseDetailsJson.Length > 3 ?
        DeserializeLicenseDetails(licenseDetailsJson).expires : string.Empty;

      return new KeyValuePair<LicenseType, string>(licenseType, expirationDate);
    }


    private AccountApiModel DeserializeUserAccountDetails(string json)
    {
      var accoutApiModel = JsonConvert.DeserializeObject<AccountApiModel>(json);
      return accoutApiModel;
    }

    private LicenseModel DeserializeLicenseDetails(string json)
    {
      var userLicenseCollection = JsonConvert.DeserializeObject<List<LicenseModel>>(json);
      return userLicenseCollection[0];
    }

    #endregion

  }
}
