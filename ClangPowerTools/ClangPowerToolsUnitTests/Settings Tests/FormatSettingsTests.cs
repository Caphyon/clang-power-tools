using Xunit;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace ClangPowerTools.Tests.Settings
{
  [VsTestSettings(UIThread = true)]
  public class FormatSettingsTests
  {
    [VsFact(Version = "2017-")]
    public async Task ClangFormatOptionsView_NotNullAsync()
    {
      //Arrange
      await UnitTestUtility.LoadPackageAsync();

      //Act
      ClangFormatOptionsView clangFormatSettings = SettingsProvider.ClangFormatSettings;

      //Assert
      Assert.NotNull(clangFormatSettings);
    }

    [VsFact(Version = "2017-")]
    public async Task FormatOnSave_ChangeValue_CompareViewToFileAsync()
    {
      await UnitTestUtility.LoadPackageAsync();
      SettingsTestUtility.ResetClangFormatOptionsView();
      ClangFormatOptionsView clangFormatSettings = SettingsProvider.ClangFormatSettings;

      clangFormatSettings.EnableFormatOnSave = true;
      SettingsTestUtility.SaveFormatOptions(clangFormatSettings);
      ClangFormatOptionsView clangFormatSettingsFromFile = SettingsTestUtility.GetClangFormatOptionsViewFromFile();

      Assert.Equal(clangFormatSettings.EnableFormatOnSave, clangFormatSettingsFromFile.EnableFormatOnSave);
    }

    [VsFact(Version = "2017-")]
    public async Task FileExtensions_ChangeValue_CompareViewToFileAsync()
    {
      await UnitTestUtility.LoadPackageAsync();
      SettingsTestUtility.ResetClangFormatOptionsView();
      ClangFormatOptionsView clangFormatSettings = SettingsProvider.ClangFormatSettings;

      clangFormatSettings.FileExtensions = ".cpp";
      SettingsTestUtility.SaveFormatOptions(clangFormatSettings);
      ClangFormatOptionsView clangFormatSettingsFromFile = SettingsTestUtility.GetClangFormatOptionsViewFromFile();

      Assert.Equal(clangFormatSettings.FileExtensions, clangFormatSettingsFromFile.FileExtensions);
    }

    [VsFact(Version = "2017-")]
    public async Task FilesToIgnore_ChangeValue_CompareViewToFileAsync()
    {
      await UnitTestUtility.LoadPackageAsync();
      SettingsTestUtility.ResetClangFormatOptionsView();
      ClangFormatOptionsView clangFormatSettings = SettingsProvider.ClangFormatSettings;

      clangFormatSettings.FilesToIgnore = "Test.cpp";
      SettingsTestUtility.SaveFormatOptions(clangFormatSettings);
      ClangFormatOptionsView clangFormatSettingsFromFile = SettingsTestUtility.GetClangFormatOptionsViewFromFile();

      Assert.Equal(clangFormatSettings.FilesToIgnore, clangFormatSettingsFromFile.FilesToIgnore);
    }

    [VsFact(Version = "2017-")]
    public async Task AssumeFilename_ChangeValue_CompareViewToFileAsync()
    {
      await UnitTestUtility.LoadPackageAsync();
      SettingsTestUtility.ResetClangFormatOptionsView();
      ClangFormatOptionsView clangFormatSettings = SettingsProvider.ClangFormatSettings;

      clangFormatSettings.AssumeFilename = "Test";
      SettingsTestUtility.SaveFormatOptions(clangFormatSettings);
      ClangFormatOptionsView clangFormatSettingsFromFile = SettingsTestUtility.GetClangFormatOptionsViewFromFile();

      Assert.Equal(clangFormatSettings.AssumeFilename, clangFormatSettingsFromFile.AssumeFilename);
    }

    [VsFact(Version = "2017-")]
    public async Task FallbackStyle_ChangeValue_CompareViewToFileAsync()
    {
      await UnitTestUtility.LoadPackageAsync();
      SettingsTestUtility.ResetClangFormatOptionsView();
      ClangFormatOptionsView clangFormatSettings = SettingsProvider.ClangFormatSettings;

      clangFormatSettings.FallbackStyle = ClangFormatFallbackStyle.Mozilla;
      SettingsTestUtility.SaveFormatOptions(clangFormatSettings);
      ClangFormatOptionsView clangFormatSettingsFromFile = SettingsTestUtility.GetClangFormatOptionsViewFromFile();

      Assert.Equal(clangFormatSettings.FallbackStyle, clangFormatSettingsFromFile.FallbackStyle);
    }

    [VsFact(Version = "2017-")]
    public async Task FormatStyle_ChangeValue_CompareViewToFileAsync()
    {
      await UnitTestUtility.LoadPackageAsync();
      SettingsTestUtility.ResetClangFormatOptionsView();
      ClangFormatOptionsView clangFormatSettings = SettingsProvider.ClangFormatSettings;

      clangFormatSettings.Style = ClangFormatStyle.Mozilla;
      SettingsTestUtility.SaveFormatOptions(clangFormatSettings);
      ClangFormatOptionsView clangFormatSettingsFromFile = SettingsTestUtility.GetClangFormatOptionsViewFromFile();

      Assert.Equal(clangFormatSettings.Style, clangFormatSettingsFromFile.Style);
    }

    [VsFact(Version = "2017-")]
    public async Task CustomExecutable_ChangeValue_CompareViewToFileAsync()
    {
      await UnitTestUtility.LoadPackageAsync();
      SettingsTestUtility.ResetClangFormatOptionsView();
      ClangFormatOptionsView clangFormatSettings = SettingsProvider.ClangFormatSettings;

      clangFormatSettings.ClangFormatPath.Enable = true;
      clangFormatSettings.ClangFormatPath.Value = @"D:\Test.exe";
      SettingsTestUtility.SaveFormatOptions(clangFormatSettings);
      ClangFormatOptionsView clangFormatSettingsFromFile = SettingsTestUtility.GetClangFormatOptionsViewFromFile();

      Assert.Equal(clangFormatSettings.ClangFormatPath.Enable, clangFormatSettingsFromFile.ClangFormatPath.Enable);
      Assert.Equal(clangFormatSettings.ClangFormatPath.Value, clangFormatSettingsFromFile.ClangFormatPath.Value);
    }
  }
}
