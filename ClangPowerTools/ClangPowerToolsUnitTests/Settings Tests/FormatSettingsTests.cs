using Xunit;

namespace ClangPowerTools.Tests.Settings
{
  [VsTestSettings(UIThread = true)]
  public class FormatSettingsTests
  {
    [VsFact(Version = "2019-")]
    public void FormatSettings_NotNull()
    {
      //Arrange
      var settingsProvider = new SettingsProvider();

      //Act
      var formatSettingsModel = settingsProvider.GetFormatSettingsModel();

      //Assert
      Assert.NotNull(formatSettingsModel);
    }

    [VsFact(Version = "2019-")]
    public void FormatOnSave_ChangeValue_CompareViewToFile()
    {
      var settingsHandler = new SettingsHandler();
      var settingsProvider = new SettingsProvider();
      var formatSettingsModel = new FormatSettingsModel
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
    public void FileExtensions_ChangeValue_CompareViewToFile()
    {

      var settingsHandler = new SettingsHandler();
      var settingsProvider = new SettingsProvider();
      var formatSettingsModel = new FormatSettingsModel
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
    public void FilesToIgnore_ChangeValue_CompareViewToFile()
    {

      var settingsHandler = new SettingsHandler();
      var settingsProvider = new SettingsProvider();
      var formatSettingsModel = new FormatSettingsModel
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
    public void AssumeFilename_ChangeValue_CompareViewToFile()
    {

      var settingsHandler = new SettingsHandler();
      var settingsProvider = new SettingsProvider();
      var formatSettingsModel = new FormatSettingsModel
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
    public void FallbackStyle_ChangeValue_CompareViewToFile()
    {

      var settingsHandler = new SettingsHandler();
      var settingsProvider = new SettingsProvider();
      var formatSettingsModel = new FormatSettingsModel
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
    public void FormatStyle_ChangeValue_CompareViewToFile()
    {

      var settingsHandler = new SettingsHandler();
      var settingsProvider = new SettingsProvider();
      var formatSettingsModel = new FormatSettingsModel
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
    public void CustomExecutable_ChangeValue_CompareViewToFile()
    {

      var settingsHandler = new SettingsHandler();
      var settingsProvider = new SettingsProvider();
      var formatSettingsModel = new FormatSettingsModel
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
