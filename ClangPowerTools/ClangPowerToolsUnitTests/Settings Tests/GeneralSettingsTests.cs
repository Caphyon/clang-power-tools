using Xunit;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;
using ClangPowerTools.DialogPages;


namespace ClangPowerTools.Tests.Settings
{
  public class GeneralSettingsTests
  {
    [VsFact(Version = "2019")]
    public async Task ClangGeneralOptionsView_NotNullAsync()
    {
      //Arrange
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();

      //Act
      ClangGeneralOptionsView generalSettings = SettingsProvider.GeneralSettings;

      //Assert
      Assert.NotNull(generalSettings);
    }
  }
}
