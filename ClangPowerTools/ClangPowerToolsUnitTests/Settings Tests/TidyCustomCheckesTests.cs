using Xunit;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace ClangPowerTools.Tests.Settings
{
  [VsTestSettings(UIThread = true)]
  public class TidyCustomCheckesTests
  {
    [VsFact(Version = "2019")]
    public async Task ClangTidyCustomChecksOptionsView_NotNullAsync()
    {
      //Arrange
      await UnitTestUtility.LoadPackageAsync();

      //Act
      ClangTidyCustomChecksOptionsView tidyCustomCheckes = SettingsProvider.TidyCustomCheckes;

      //Assert
      Assert.NotNull(tidyCustomCheckes);
    }

    [VsFact(Version = "2019")]
    public async Task CustomChecks_ChangeValue_CompareViewToFileAsync()
    {
      await UnitTestUtility.LoadPackageAsync();
      UnitTestUtility.ResetClangTidyOptionsView();
      ClangTidyCustomChecksOptionsView tidyCustomCheckes = SettingsProvider.TidyCustomCheckes;

      tidyCustomCheckes.TidyChecks = "test";
      UnitTestUtility.SaveClangTidyCustomChecksOptionsView(tidyCustomCheckes);
      ClangTidyCustomChecksOptionsView tidyCustomCheckesFromFile = UnitTestUtility.GetClangTidyCustomChecksViewFromFile();

      Assert.Equal(tidyCustomCheckes.TidyChecks, tidyCustomCheckesFromFile.TidyChecks);
    }
  }
}
