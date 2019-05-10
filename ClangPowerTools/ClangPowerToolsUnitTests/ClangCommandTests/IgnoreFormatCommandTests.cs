using System.Collections.Generic;
using System.IO;
using ClangPowerTools.Commands;
using ClangPowerToolsUnitTests.Constants;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Xunit;

namespace ClangPowerTools.Tests
{
  [VsTestSettings(UIThread = true)]
  public class IgnoreFormatCommandTests
  {
    #region Members

    private const string kFormatSettingsPath = @"C:\Users\Enache Ionut\AppData\Roaming\ClangPowerTools\FormatConfiguration.config";

    private DTE2 mDte;
    private IgnoreFormatCommand mIgnoreFormatCommand;
    private ClangFormatOptionsView mFormatOptions;

    #endregion

    #region Methods

    #region Public Methods

    // Empty State 

    [VsFact(Version = "2017-")]
    public async System.Threading.Tasks.Task SaveFilesToIgnore_EmptyState_UIAsync()
    {
      //Arrange
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();

      // Act
      Initialize(string.Empty);
      await IgnoreFilesAsync(IgnoreCommand.kSingleFileToIgnore);
      SettingsHandler.SaveFormatSettings();

      var filesToIgnore = mFormatOptions.FilesToIgnore;
      var expectedRestul = string.Join(";", IgnoreCommand.kSingleFileToIgnore);
      SettingsTestUtility.ResetClangFormatOptionsView();

      // Assert
      Assert.Equal(filesToIgnore, expectedRestul);
    }


    [VsFact(Version = "2017-")]
    public async System.Threading.Tasks.Task SaveFilesToIgnore_EmptyState_ConfigAsync()
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();

      Initialize(string.Empty);
      await IgnoreFilesAsync(IgnoreCommand.kSingleFileToIgnore);
      SettingsHandler.SaveFormatSettings();

      if (!File.Exists(kFormatSettingsPath))
        Assert.False(true);

      XmlSerializer serializer = new XmlSerializer();
      var formatSettingsModel = serializer.DeserializeFromFile<ClangFormatOptions>(kFormatSettingsPath);

      var filesToIgnore = formatSettingsModel.SkipFiles;
      var expectedRestul = string.Join(";", IgnoreCommand.kSingleFileToIgnore);
      SettingsTestUtility.ResetClangFormatOptionsView();

      Assert.Equal(filesToIgnore, expectedRestul);
    }


    [VsFact(Version = "2017-")]
    public async System.Threading.Tasks.Task SaveMultipleFilesToIgnore_EmptyState_UIAsync()
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();

      Initialize(string.Empty);
      await IgnoreFilesAsync(IgnoreCommand.kMultipleFilesToIgnore);
      SettingsHandler.SaveFormatSettings();

      var filesToIgnore = mFormatOptions.FilesToIgnore;
      var expectedRestul = string.Join(";", IgnoreCommand.kMultipleFilesToIgnore);
      SettingsTestUtility.ResetClangFormatOptionsView();

      Assert.Equal(filesToIgnore, expectedRestul);
    }


    [VsFact(Version = "2017-")]
    public async System.Threading.Tasks.Task SaveMultipleFilesToIgnore_EmptyState_ConfigAsync()
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();

      Initialize(string.Empty);
      await IgnoreFilesAsync(IgnoreCommand.kMultipleFilesToIgnore);
      SettingsHandler.SaveFormatSettings();

      if (!File.Exists(kFormatSettingsPath))
        Assert.False(true);

      XmlSerializer serializer = new XmlSerializer();
      var formatSettingsModel = serializer.DeserializeFromFile<ClangFormatOptions>(kFormatSettingsPath);

      var filesToIgnore = formatSettingsModel.SkipFiles;
      var expectedRestul = string.Join(";", IgnoreCommand.kMultipleFilesToIgnore);
      SettingsTestUtility.ResetClangFormatOptionsView();

      Assert.Equal(filesToIgnore, expectedRestul);
    }


    // No Empty State

    [VsFact(Version = "2017-")]
    public async System.Threading.Tasks.Task SaveFilesToIgnore_NoEmptyState_UIAsync()
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();

      var expectedResult = string.Join(";", IgnoreCommand.kStartUpMultipleFilesToIgnore);
      Initialize(expectedResult);
      await IgnoreFilesAsync(IgnoreCommand.kMultipleFilesToIgnore);
      SettingsHandler.SaveFormatSettings();

      expectedResult += ";" + string.Join(";", IgnoreCommand.kMultipleFilesToIgnore);

      var filesToIgnore = mFormatOptions.FilesToIgnore;
      SettingsTestUtility.ResetClangFormatOptionsView();

      Assert.Equal(filesToIgnore, expectedResult);
    }


    [VsFact(Version = "2017-")]
    public async System.Threading.Tasks.Task SaveFilesToIgnore_NoEmptyState_ConfigAsync()
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();

      var expectedResult = string.Join(";", IgnoreCommand.kStartUpMultipleFilesToIgnore);
      Initialize(expectedResult);
      await IgnoreFilesAsync(IgnoreCommand.kMultipleFilesToIgnore);
      SettingsHandler.SaveFormatSettings();

      if (!File.Exists(kFormatSettingsPath))
        Assert.False(true);

      XmlSerializer serializer = new XmlSerializer();

      var formatSettingsModel = serializer.DeserializeFromFile<ClangFormatOptions>(kFormatSettingsPath);
      expectedResult += ";" + string.Join(";", IgnoreCommand.kMultipleFilesToIgnore);

      var filesToIgnore = formatSettingsModel.SkipFiles;
      SettingsTestUtility.ResetClangFormatOptionsView();

      Assert.Equal(filesToIgnore, expectedResult);
    }

    #endregion


    #region Private Methods

    private void Initialize(string ignoreFiles)
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      mDte = (DTE2)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
      SettingsTestUtility.ResetClangFormatOptionsView();

      mIgnoreFormatCommand = IgnoreFormatCommand.Instance;
      mFormatOptions = SettingsProvider.ClangFormatSettings;
      mFormatOptions.FilesToIgnore = ignoreFiles;
    }

    private async System.Threading.Tasks.Task IgnoreFilesAsync(List<string> aFilesToIgnore)
    {
      await System.Threading.Tasks.Task.Run(() =>
      {
        mIgnoreFormatCommand.AddIgnoreFilesToSettings(aFilesToIgnore);
      });
    }

    #endregion


    #endregion


  }
}
