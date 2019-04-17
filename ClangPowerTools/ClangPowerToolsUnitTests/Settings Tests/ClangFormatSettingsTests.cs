using Xunit;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace ClangPowerTools.Tests.Settings
{
  public class ClangFormatSettingsTests
  {
    [VsFact(Version = "2019")]
    public async Task ClangFormatOptionsView_NotNullAsync()
    {
      //Arrange
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();

      //Act
      ClangFormatOptionsView clangFormatSettings = SettingsProvider.ClangFormatSettings;

      //Assert
      Assert.NotNull(clangFormatSettings);
    }

    [VsFact(Version = "2019")]
    public async Task FormatOnSave_ChangeValue_CompareViewToFileAsync()
    {
      //Arrange
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();
      UnitTestUtility.ResetClangFormatOptionsView();
      ClangFormatOptionsView clangFormatSettings = SettingsProvider.ClangFormatSettings;

      //Act
      clangFormatSettings.EnableFormatOnSave = true;
      UnitTestUtility.SaveFormatOptions(clangFormatSettings);
      ClangFormatOptionsView clangFormatSettingsFromFile = UnitTestUtility.GetClangFormatOptionsViewFromFile();

      //Assert
      Assert.Equal(clangFormatSettings.EnableFormatOnSave, clangFormatSettingsFromFile.EnableFormatOnSave);
    }

  }
}
