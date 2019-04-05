using Xunit;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;
using ClangPowerTools.DialogPages;

namespace ClangPowerTools.Tests.Settings
{
  public class TidyCustomCheckesTests
  {
    [VsFact(Version = "2019")]
    public async Task ClangTidyCustomChecksOptionsView_NotNullAsync()
    {
      //Arrange
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();

      //Act
      ClangTidyCustomChecksOptionsView tidyCustomCheckes = SettingsProvider.TidyCustomCheckes;

      //Assert
      Assert.NotNull(tidyCustomCheckes);
    }
  }
}
