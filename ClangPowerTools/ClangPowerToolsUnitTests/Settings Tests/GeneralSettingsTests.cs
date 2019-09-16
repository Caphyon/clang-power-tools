using Xunit;

namespace ClangPowerTools.Tests.Settings
{
  [VsTestSettings(UIThread = true)]
  public class GeneralSettingsTests
  {
    [VsFact(Version = "2019-")]
    public void GeneralSettings_NotNull()
    {
      //Arrange
      var settingsProvider = new SettingsProvider();

      //Act
      var generalSettingsModel = settingsProvider.GetGeneralSettingsModel();

      //Assert
      Assert.NotNull(generalSettingsModel);
    }

    [VsFact(Version ="2019-")]
    public void Version_ChangeValue_CompareViewToFile()
    {
      var settingsHandler = new SettingsHandler();
      var settingsProvider = new SettingsProvider();
      var generalSettingsModel = new GeneralSettingsModel
      {
        Version = "5.0.0"
      };

      settingsProvider.SetGeneralSettingsModel(generalSettingsModel);
      settingsHandler.SaveSettings();
      settingsHandler.ResetSettings();
      settingsHandler.LoadSettings();

      Assert.Equal(generalSettingsModel.Version, settingsProvider.GetGeneralSettingsModel().Version);
    }
  }
}
