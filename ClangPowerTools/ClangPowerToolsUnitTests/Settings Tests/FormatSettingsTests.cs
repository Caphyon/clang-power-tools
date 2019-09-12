using Xunit;
using Task = System.Threading.Tasks.Task;

namespace ClangPowerTools.Tests.Settings
{
  [VsTestSettings(UIThread = true)]
  public class FormatSettingsTests
  {
    [VsFact(Version = "2019-")]
    public async Task ClangFormatOptionsView_NotNullAsync()
    {
      //Arrange
      await UnitTestUtility.LoadPackageAsync();

      //Act
      SettingsProvider settingsProvider = new SettingsProvider();
 
      //Assert
      Assert.NotNull(settingsProvider.GetFormatSettingsModel());
    }

    [VsFact(Version = "2019-")]
    public async Task FormatOnSave_ChangeValue_CompareViewToFileAsync()
    {
      await UnitTestUtility.LoadPackageAsync();
      SettingsHandler settingsHandler = new SettingsHandler();
      SettingsProvider settingsProvider = new SettingsProvider();
      FormatSettingsModel formatSettingsModel = new FormatSettingsModel
      {
        FormatOnSave = true
      };

      settingsProvider.SetFormatSettingsModel(formatSettingsModel);
      settingsHandler.SaveSettings();
      settingsHandler.ResetSettings();
      settingsHandler.LoadSettings();

      Assert.Equal(formatSettingsModel.FormatOnSave, settingsProvider.GetFormatSettingsModel().FormatOnSave);
    }

    [VsFact(Version = "2019-")]
    public async Task FileExtensions_ChangeValue_CompareViewToFileAsync()
    {
      await UnitTestUtility.LoadPackageAsync();
      SettingsHandler settingsHandler = new SettingsHandler();
      SettingsProvider settingsProvider = new SettingsProvider();
      FormatSettingsModel formatSettingsModel = new FormatSettingsModel
      {
        FileExtensions = ".cpp"
      };

      settingsProvider.SetFormatSettingsModel(formatSettingsModel);
      settingsHandler.SaveSettings();
      settingsHandler.ResetSettings();
      settingsHandler.LoadSettings();

      Assert.Equal(formatSettingsModel.FileExtensions, settingsProvider.GetFormatSettingsModel().FileExtensions);
    }

    [VsFact(Version = "2019-")]
    public async Task FilesToIgnore_ChangeValue_CompareViewToFileAsync()
    {
      await UnitTestUtility.LoadPackageAsync();
      SettingsHandler settingsHandler = new SettingsHandler();
      SettingsProvider settingsProvider = new SettingsProvider();
      FormatSettingsModel formatSettingsModel = new FormatSettingsModel
      {
        FilesToIgnore = "Test.cpp"
    };

      settingsProvider.SetFormatSettingsModel(formatSettingsModel);
      settingsHandler.SaveSettings();
      settingsHandler.ResetSettings();
      settingsHandler.LoadSettings();

      Assert.Equal(formatSettingsModel.FilesToIgnore, settingsProvider.GetFormatSettingsModel().FilesToIgnore);
    }

    [VsFact(Version = "2019-")]
    public async Task AssumeFilename_ChangeValue_CompareViewToFileAsync()
    {
      await UnitTestUtility.LoadPackageAsync();
      SettingsHandler settingsHandler = new SettingsHandler();
      SettingsProvider settingsProvider = new SettingsProvider();
      FormatSettingsModel formatSettingsModel = new FormatSettingsModel
      {
        AssumeFilename = "Test"
      };

      settingsProvider.SetFormatSettingsModel(formatSettingsModel);
      settingsHandler.SaveSettings();
      settingsHandler.ResetSettings();
      settingsHandler.LoadSettings();

      Assert.Equal(formatSettingsModel.AssumeFilename, settingsProvider.GetFormatSettingsModel().AssumeFilename);
    }

    [VsFact(Version = "2019-")]
    public async Task FallbackStyle_ChangeValue_CompareViewToFileAsync()
    {
      await UnitTestUtility.LoadPackageAsync();
      SettingsHandler settingsHandler = new SettingsHandler();
      SettingsProvider settingsProvider = new SettingsProvider();
      FormatSettingsModel formatSettingsModel = new FormatSettingsModel
      {
        FallbackStyle = ClangFormatFallbackStyle.Mozilla
      };

      settingsProvider.SetFormatSettingsModel(formatSettingsModel);
      settingsHandler.SaveSettings();
      settingsHandler.ResetSettings();
      settingsHandler.LoadSettings();

      Assert.Equal(formatSettingsModel.FallbackStyle, settingsProvider.GetFormatSettingsModel().FallbackStyle);
    }

    [VsFact(Version = "2019-")]
    public async Task FormatStyle_ChangeValue_CompareViewToFileAsync()
    {
      await UnitTestUtility.LoadPackageAsync();
      SettingsHandler settingsHandler = new SettingsHandler();
      SettingsProvider settingsProvider = new SettingsProvider();
      FormatSettingsModel formatSettingsModel = new FormatSettingsModel
      {
        Style = ClangFormatStyle.Mozilla
      };

      settingsProvider.SetFormatSettingsModel(formatSettingsModel);
      settingsHandler.SaveSettings();
      settingsHandler.ResetSettings();
      settingsHandler.LoadSettings();

      Assert.Equal(formatSettingsModel.Style, settingsProvider.GetFormatSettingsModel().Style);
    }

    [VsFact(Version = "2019-")]
    public async Task CustomExecutable_ChangeValue_CompareViewToFileAsync()
    {
      await UnitTestUtility.LoadPackageAsync();
      SettingsHandler settingsHandler = new SettingsHandler();
      SettingsProvider settingsProvider = new SettingsProvider();
      FormatSettingsModel formatSettingsModel = new FormatSettingsModel
      {
        CustomExecutable = @"D:\Test.exe"
    };

      settingsProvider.SetFormatSettingsModel(formatSettingsModel);
      settingsHandler.SaveSettings();
      settingsHandler.ResetSettings();
      settingsHandler.LoadSettings();

      Assert.Equal(formatSettingsModel.CustomExecutable, settingsProvider.GetFormatSettingsModel().CustomExecutable);
    }
  }
}
