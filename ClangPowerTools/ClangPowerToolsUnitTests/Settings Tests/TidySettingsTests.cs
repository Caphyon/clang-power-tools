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
      SettingsProvider settingsProvider = new SettingsProvider();

      //Act
      TidySettingsModel tidySettingsModel = settingsProvider.GetTidySettingsModel();

      //Assert
      Assert.NotNull(tidySettingsModel);
    }

    [VsFact(Version = "2019-")]
    public void FormatAfterTidy_ChangeValue_CompareViewToFile()
    {
      SettingsHandler settingsHandler = new SettingsHandler();
      SettingsProvider settingsProvider = new SettingsProvider();
      TidySettingsModel tidySettingsModel = new TidySettingsModel
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
      SettingsHandler settingsHandler = new SettingsHandler();
      SettingsProvider settingsProvider = new SettingsProvider();
      TidySettingsModel tidySettingsModel = new TidySettingsModel
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
      SettingsHandler settingsHandler = new SettingsHandler();
      SettingsProvider settingsProvider = new SettingsProvider();
      TidySettingsModel tidySettingsModel = new TidySettingsModel
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
      SettingsHandler settingsHandler = new SettingsHandler();
      SettingsProvider settingsProvider = new SettingsProvider();
      TidySettingsModel tidySettingsModel = new TidySettingsModel
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
      SettingsHandler settingsHandler = new SettingsHandler();
      SettingsProvider settingsProvider = new SettingsProvider();
      TidySettingsModel tidySettingsModel = new TidySettingsModel
      {
        CustomExecutable = @"D:\Test.exe"
      };

      settingsProvider.SetTidySettingsModel(tidySettingsModel);
      settingsHandler.SaveSettings();
      settingsHandler.ResetSettings();
      settingsHandler.LoadSettings();

      Assert.Equal(tidySettingsModel.CustomExecutable, settingsProvider.GetTidySettingsModel().CustomExecutable);
    }
  }
}
