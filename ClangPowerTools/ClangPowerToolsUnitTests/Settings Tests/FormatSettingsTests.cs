using Xunit;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace ClangPowerTools.Tests.Settings
{
  public class FormatSettingsTests
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

    [VsFact(Version = "2019")]
    public async Task FileExtensions_ChangeValue_CompareViewToFileAsync()
    {
      //Arrange
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();
      UnitTestUtility.ResetClangFormatOptionsView();
      ClangFormatOptionsView clangFormatSettings = SettingsProvider.ClangFormatSettings;

      //Act
      clangFormatSettings.FileExtensions = ".cpp";
      UnitTestUtility.SaveFormatOptions(clangFormatSettings);
      ClangFormatOptionsView clangFormatSettingsFromFile = UnitTestUtility.GetClangFormatOptionsViewFromFile();

      //Assert
      Assert.Equal(clangFormatSettings.FileExtensions, clangFormatSettingsFromFile.FileExtensions);
    }

    [VsFact(Version = "2019")]
    public async Task FilesToIgnore_ChangeValue_CompareViewToFileAsync()
    {
      //Arrange
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();
      UnitTestUtility.ResetClangFormatOptionsView();
      ClangFormatOptionsView clangFormatSettings = SettingsProvider.ClangFormatSettings;

      //Act
      clangFormatSettings.FilesToIgnore = "Test.cpp";
      UnitTestUtility.SaveFormatOptions(clangFormatSettings);
      ClangFormatOptionsView clangFormatSettingsFromFile = UnitTestUtility.GetClangFormatOptionsViewFromFile();

      //Assert
      Assert.Equal(clangFormatSettings.FilesToIgnore, clangFormatSettingsFromFile.FilesToIgnore);
    }

    [VsFact(Version = "2019")]
    public async Task AssumeFilename_ChangeValue_CompareViewToFileAsync()
    {
      //Arrange
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();
      UnitTestUtility.ResetClangFormatOptionsView();
      ClangFormatOptionsView clangFormatSettings = SettingsProvider.ClangFormatSettings;

      //Act
      clangFormatSettings.AssumeFilename = "Test";
      UnitTestUtility.SaveFormatOptions(clangFormatSettings);
      ClangFormatOptionsView clangFormatSettingsFromFile = UnitTestUtility.GetClangFormatOptionsViewFromFile();

      //Assert
      Assert.Equal(clangFormatSettings.AssumeFilename, clangFormatSettingsFromFile.AssumeFilename);
    }

    [VsFact(Version = "2019")]
    public async Task FallbackStyle_ChangeValue_CompareViewToFileAsync()
    {
      //Arrange
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();
      UnitTestUtility.ResetClangFormatOptionsView();
      ClangFormatOptionsView clangFormatSettings = SettingsProvider.ClangFormatSettings;

      //Act
      clangFormatSettings.FallbackStyle = ClangFormatFallbackStyle.Mozilla;
      UnitTestUtility.SaveFormatOptions(clangFormatSettings);
      ClangFormatOptionsView clangFormatSettingsFromFile = UnitTestUtility.GetClangFormatOptionsViewFromFile();

      //Assert
      Assert.Equal(clangFormatSettings.FallbackStyle, clangFormatSettingsFromFile.FallbackStyle);
    }

    [VsFact(Version = "2019")]
    public async Task FormatStyle_ChangeValue_CompareViewToFileAsync()
    {
      //Arrange
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();
      UnitTestUtility.ResetClangFormatOptionsView();
      ClangFormatOptionsView clangFormatSettings = SettingsProvider.ClangFormatSettings;

      //Act
      clangFormatSettings.Style = ClangFormatStyle.Mozilla;
      UnitTestUtility.SaveFormatOptions(clangFormatSettings);
      ClangFormatOptionsView clangFormatSettingsFromFile = UnitTestUtility.GetClangFormatOptionsViewFromFile();

      //Assert
      Assert.Equal(clangFormatSettings.Style, clangFormatSettingsFromFile.Style);
    }

    [VsFact(Version = "2019")]
    public async Task CustomExecutable_ChangeValue_CompareViewToFileAsync()
    {
      //Arrange
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();
      UnitTestUtility.ResetClangFormatOptionsView();
      ClangFormatOptionsView clangFormatSettings = SettingsProvider.ClangFormatSettings;

      //Act
      clangFormatSettings.ClangFormatPath.Enable = true;
      clangFormatSettings.ClangFormatPath.Value = @"D:\Test.exe";
      UnitTestUtility.SaveFormatOptions(clangFormatSettings);
      ClangFormatOptionsView clangFormatSettingsFromFile = UnitTestUtility.GetClangFormatOptionsViewFromFile();

      //Assert
      Assert.Equal(clangFormatSettings.ClangFormatPath.Enable, clangFormatSettingsFromFile.ClangFormatPath.Enable);
      Assert.Equal(clangFormatSettings.ClangFormatPath.Value, clangFormatSettingsFromFile.ClangFormatPath.Value);
    }
  }
}
