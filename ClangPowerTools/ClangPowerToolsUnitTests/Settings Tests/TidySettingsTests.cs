using Xunit;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace ClangPowerTools.Tests.Settings
{
  [VsTestSettings(UIThread = true)]
  public class TidySettingsTests
  {
    [VsFact(Version = "2017-")]
    public async Task ClangTidyOptionsView_NotNullAsync()
    {
      //Arrange
      await UnitTestUtility.LoadPackageAsync();

      //Act
      ClangTidyOptionsView tidySettings = SettingsProvider.TidySettings;

      //Assert
      Assert.NotNull(tidySettings);
    }

    [VsFact(Version = "2017-")]
    public async Task FormatAfterTidy_ChangeValue_CompareViewToFileAsync()
    {
      await UnitTestUtility.LoadPackageAsync();
      SettingsTestUtility.ResetClangTidyOptionsView();
      ClangTidyOptionsView tidySettings = SettingsProvider.TidySettings;

      tidySettings.FormatAfterTidy = true;
      SettingsTestUtility.SaveTidyOptions(tidySettings);
      ClangTidyOptionsView clangTidySettingsFromFile = SettingsTestUtility.GetClangTidyOptionViewFromFile();

      Assert.Equal(tidySettings.FormatAfterTidy, clangTidySettingsFromFile.FormatAfterTidy);
    }

    [VsFact(Version = "2017-")]
    public async Task ClangTidyOnSave_ChangeValue_CompareViewToFileAsync()
    {
      await UnitTestUtility.LoadPackageAsync();
      SettingsTestUtility.ResetClangTidyOptionsView();
      ClangTidyOptionsView tidySettings = SettingsProvider.TidySettings;

      tidySettings.AutoTidyOnSave = true;
      SettingsTestUtility.SaveTidyOptions(tidySettings);
      ClangTidyOptionsView clangTidySettingsFromFile = SettingsTestUtility.GetClangTidyOptionViewFromFile();

      Assert.Equal(tidySettings.AutoTidyOnSave, clangTidySettingsFromFile.AutoTidyOnSave);
    }

    [VsFact(Version = "2017-")]
    public async Task HeaderFilter_ChangeValue_CompareViewToFileAsync()
    {
      await UnitTestUtility.LoadPackageAsync();
      SettingsTestUtility.ResetClangTidyOptionsView();
      ClangTidyOptionsView tidySettings = SettingsProvider.TidySettings;

      tidySettings.HeaderFilter.HeaderFilters = "test";
      SettingsTestUtility.SaveTidyOptions(tidySettings);
      ClangTidyOptionsView clangTidySettingsFromFile = SettingsTestUtility.GetClangTidyOptionViewFromFile();

      Assert.Equal(tidySettings.HeaderFilter.HeaderFilters, clangTidySettingsFromFile.HeaderFilter.HeaderFilters);
    }

    [VsFact(Version = "2017-")]
    public async Task ChecksFrom_ChangeValue_CompareViewToFileAsync()
    {
      await UnitTestUtility.LoadPackageAsync();
      SettingsTestUtility.ResetClangTidyOptionsView();
      ClangTidyOptionsView tidySettings = SettingsProvider.TidySettings;

      tidySettings.UseChecksFrom = ClangTidyUseChecksFrom.TidyFile;
      SettingsTestUtility.SaveTidyOptions(tidySettings);
      ClangTidyOptionsView clangTidySettingsFromFile = SettingsTestUtility.GetClangTidyOptionViewFromFile();

      Assert.Equal(tidySettings.UseChecksFrom, clangTidySettingsFromFile.UseChecksFrom);
    }

    [VsFact(Version = "2017-")]
    public async Task CustomExecutable_ChangeValue_CompareViewToFileAsync()
    {
      await UnitTestUtility.LoadPackageAsync();
      SettingsTestUtility.ResetClangTidyOptionsView();
      ClangTidyOptionsView tidySettings = SettingsProvider.TidySettings;

      tidySettings.ClangTidyPath.Enable = true;
      tidySettings.ClangTidyPath.Value = @"D:\Test.exe";
      SettingsTestUtility.SaveTidyOptions(tidySettings);
      ClangTidyOptionsView clangTidySettingsFromFile = SettingsTestUtility.GetClangTidyOptionViewFromFile();

      Assert.Equal(tidySettings.ClangTidyPath.Enable, clangTidySettingsFromFile.ClangTidyPath.Enable);
      Assert.Equal(tidySettings.ClangTidyPath.Value, clangTidySettingsFromFile.ClangTidyPath.Value);
    }
  }
}
