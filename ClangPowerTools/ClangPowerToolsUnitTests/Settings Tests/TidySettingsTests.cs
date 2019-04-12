using Xunit;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace ClangPowerTools.Tests.Settings
{
  public class TidySettingsTests
  {
    [VsFact(Version = "2019")]
    public async Task ClangTidyOptionsView_NotNullAsync()
    {
      //Arrange
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();

      //Act
      ClangTidyOptionsView tidySettings = SettingsProvider.TidySettings;

      //Assert
      Assert.NotNull(tidySettings);
    }
  }
}
