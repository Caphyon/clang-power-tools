using Xunit;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;
using ClangPowerTools.DialogPages;

namespace ClangPowerTools.Tests.Settings
{
  public class TidyPredefinedChecksTests
  {
    [VsFact(Version = "2019")]
    public async Task ClangTidyPredefinedChecksOptionsView_NotNullAsync()
    {
      //Arrange
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();

      //Act
      ClangTidyPredefinedChecksOptionsView tidyPredefinedChecks = SettingsProvider.TidyPredefinedChecks;

      //Assert
      Assert.NotNull(tidyPredefinedChecks);
    }
  }
}
