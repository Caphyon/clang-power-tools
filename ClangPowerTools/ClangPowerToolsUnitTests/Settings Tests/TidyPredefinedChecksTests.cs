using Xunit;
using Task = System.Threading.Tasks.Task;

namespace ClangPowerTools.Tests.Settings
{
  [VsTestSettings(UIThread = true)]
  public class TidyPredefinedChecksTests
  {
    [VsFact(Version = "2017-")]
    public async Task ClangTidyPredefinedChecksOptionsView_NotNullAsync()
    {
      //Arrange
      await UnitTestUtility.LoadPackageAsync();

      //Act
      ClangTidyPredefinedChecksOptionsView tidyPredefinedChecks = SettingsProvider.TidyPredefinedChecks;

      //Assert
      Assert.NotNull(tidyPredefinedChecks);
    }

    [VsFact(Version = "2017-")]
    public async Task PredefinedChecks_ChangeValue_CompareViewToFileAsync()
    {
      await UnitTestUtility.LoadPackageAsync();
      SettingsTestUtility.ResetClangTidyPredefinedChecksOptionsView();
      ClangTidyPredefinedChecksOptionsView tidyPredefinedChecksOptionsView = SettingsProvider.TidyPredefinedChecks;

      tidyPredefinedChecksOptionsView.AbseilDurationDivision = true;
      SettingsTestUtility.SaveClangTidyPredefinedChecksOptionsView(tidyPredefinedChecksOptionsView);
      ClangTidyPredefinedChecksOptionsView tidyCustomCheckesFromFile = SettingsTestUtility.GetClangTidyPredefinedChecksOptionsView();

      Assert.Equal(tidyPredefinedChecksOptionsView.AbseilDurationDivision, tidyCustomCheckesFromFile.AbseilDurationDivision);
    }
  }
}
