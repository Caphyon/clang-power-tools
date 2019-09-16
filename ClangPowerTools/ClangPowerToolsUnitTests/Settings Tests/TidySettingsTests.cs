using Xunit;

namespace ClangPowerTools.Tests.Settings
{
  [VsTestSettings(UIThread = true)]
  public class TidySettingsTests
  {
    [VsFact(Version = "2019-")]
    public void TidySettings_NotNull()
    {
      //Arrange
      var settingsProvider = new SettingsProvider();

      //Act
      var tidySettingsModel = settingsProvider.GetTidySettingsModel();

      //Assert
      Assert.NotNull(tidySettingsModel);
    }

    [VsFact(Version = "2019-")]
    public void FormatAfterTidy_ChangeValue_CompareViewToFile()
    {
      var settingsHandler = new SettingsHandler();
      var settingsProvider = new SettingsProvider();
      var tidySettingsModel = new TidySettingsModel
      {
        FormatAfterTidy = true
      };

      settingsProvider.SetTidySettingsModel(tidySettingsModel);
      settingsHandler.SaveSettings();
      settingsHandler.ResetSettings();
      settingsHandler.LoadSettings();

      Assert.Equal(tidySettingsModel.FormatAfterTidy, settingsProvider.GetTidySettingsModel().FormatAfterTidy);
    }

    [VsFact(Version = "2019-")]
    public void ClangTidyOnSave_ChangeValue_CompareViewToFile()
    {
      var settingsHandler = new SettingsHandler();
      var settingsProvider = new SettingsProvider();
      var tidySettingsModel = new TidySettingsModel
      {
        TidyOnSave = true
      };

      settingsProvider.SetTidySettingsModel(tidySettingsModel);
      settingsHandler.SaveSettings();
      settingsHandler.ResetSettings();
      settingsHandler.LoadSettings();

      Assert.Equal(tidySettingsModel.TidyOnSave, settingsProvider.GetTidySettingsModel().TidyOnSave);
    }

    [VsFact(Version = "2019-")]
    public void HeaderFilter_ChangeValue_CompareViewToFile()
    {
      var settingsHandler = new SettingsHandler();
      var settingsProvider = new SettingsProvider();
      var tidySettingsModel = new TidySettingsModel
      {
        HeaderFilter = "test"
      };

      settingsProvider.SetTidySettingsModel(tidySettingsModel);
      settingsHandler.SaveSettings();
      settingsHandler.ResetSettings();
      settingsHandler.LoadSettings();

      Assert.Equal(tidySettingsModel.HeaderFilter, settingsProvider.GetTidySettingsModel().HeaderFilter);
    }

    [VsFact(Version = "2019-")]
    public void ChecksFrom_ChangeValue_CompareViewToFile()
    {
      var settingsHandler = new SettingsHandler();
      var settingsProvider = new SettingsProvider();
      var tidySettingsModel = new TidySettingsModel
      {
        UseChecksFrom = ClangTidyUseChecksFrom.TidyFile
      };

      settingsProvider.SetTidySettingsModel(tidySettingsModel);
      settingsHandler.SaveSettings();
      settingsHandler.ResetSettings();
      settingsHandler.LoadSettings();

      Assert.Equal(tidySettingsModel.UseChecksFrom, settingsProvider.GetTidySettingsModel().UseChecksFrom);
    }

    [VsFact(Version = "2019-")]
    public void CustomExecutable_ChangeValue_CompareViewToFile()
    {
      var settingsHandler = new SettingsHandler();
      var settingsProvider = new SettingsProvider();
      var tidySettingsModel = new TidySettingsModel
      {
        CustomExecutable = @"D:\Test.exe"
      };

      settingsProvider.SetTidySettingsModel(tidySettingsModel);
      settingsHandler.SaveSettings();
      settingsHandler.ResetSettings();
      settingsHandler.LoadSettings();

      Assert.Equal(tidySettingsModel.CustomExecutable, settingsProvider.GetTidySettingsModel().CustomExecutable);
    }

    [VsFact(Version = "2019-")]
    public void PredefinedChecks_ChangeValue_CompareViewToFile()
    {
      var settingsHandler = new SettingsHandler();
      var settingsProvider = new SettingsProvider();
      var tidySettingsModel = new TidySettingsModel
      {
        CustomExecutable = @"D:\Test.exe"
      };

      settingsProvider.SetTidySettingsModel(tidySettingsModel);
      settingsHandler.SaveSettings();
      settingsHandler.ResetSettings();
      settingsHandler.LoadSettings();

      Assert.Equal(tidySettingsModel.CustomExecutable, settingsProvider.GetTidySettingsModel().CustomExecutable);
    }

    [VsFact(Version = "2019-")]
    public void CustomChecks_ChangeValue_CompareViewToFile()
    {
      var settingsHandler = new SettingsHandler();
      var settingsProvider = new SettingsProvider();
      var tidySettingsModel = new TidySettingsModel
      {
        CustomChecks = "test-check"
      };

      settingsProvider.SetTidySettingsModel(tidySettingsModel);
      settingsHandler.SaveSettings();
      settingsHandler.ResetSettings();
      settingsHandler.LoadSettings();

      Assert.Equal(tidySettingsModel.CustomChecks, settingsProvider.GetTidySettingsModel().CustomChecks);
    }
  }
}
