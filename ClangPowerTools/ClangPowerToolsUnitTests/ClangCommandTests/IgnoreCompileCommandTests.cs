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
  public class IgnoreCompileCommandTests
  {
    #region Members

    private const string kGeneralSettingsPath = @"C:\Users\Enache Ionut\AppData\Roaming\ClangPowerTools\GeneralConfiguration.config";

    private DTE2 mDte;
    private IgnoreCompileCommand mIgnoreCompileCommand;
    private ClangGeneralOptionsView mGeneralOptions;

    #endregion


    #region Test Methods

    // Empty State 

    [VsFact(Version = "2017-")]
    public async System.Threading.Tasks.Task SaveFilesToIgnore_EmptyState_UIAsync()
    {
      //Arrange
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();

      //Act
      Initialize(string.Empty);
      await IgnoreFilesAsync(IgnoreCommand.kSingleFileToIgnore);
      SettingsHandler.SaveGeneralSettings();

      var filesToIgnore = mGeneralOptions.FilesToIgnore;
      var expectedRestul = string.Join(";", IgnoreCommand.kSingleFileToIgnore);
      SettingsTestUtility.ResetClangGeneralOptionsView();

      //Assert
      Assert.Equal(filesToIgnore, expectedRestul);
    }


    [VsFact(Version = "2017-")]
    public async System.Threading.Tasks.Task SaveFilesToIgnore_EmptyState_ConfigAsync()
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();

      Initialize(string.Empty);
      await IgnoreFilesAsync(IgnoreCommand.kSingleFileToIgnore);
      SettingsHandler.SaveGeneralSettings();

      if (!File.Exists(kGeneralSettingsPath))
        Assert.False(true);

      XmlSerializer serializer = new XmlSerializer();
      var generalSettingsModel = serializer.DeserializeFromFile<ClangOptions>(kGeneralSettingsPath);

      var filesToIgnore = generalSettingsModel.FilesToIgnoreCollection;
      var expectedRestul = string.Join(";", IgnoreCommand.kSingleFileToIgnore);
      SettingsTestUtility.ResetClangGeneralOptionsView();

      Assert.Equal(filesToIgnore, expectedRestul);
    }


    [VsFact(Version = "2017-")]
    public async System.Threading.Tasks.Task SaveMultipleFilesToIgnore_EmptyState_UIAsync()
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();

      Initialize(string.Empty);
      await IgnoreFilesAsync(IgnoreCommand.kMultipleFilesToIgnore);
      SettingsHandler.SaveGeneralSettings();

      var filesToIgnore = mGeneralOptions.FilesToIgnore;
      var expectedRestul = string.Join(";", IgnoreCommand.kMultipleFilesToIgnore);
      SettingsTestUtility.ResetClangGeneralOptionsView();

      Assert.Equal(filesToIgnore, expectedRestul);
    }


    [VsFact(Version = "2017-")]
    public async System.Threading.Tasks.Task SaveMultipleFilesToIgnore_EmptyState_ConfigAsync()
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();

      Initialize(string.Empty);
      await IgnoreFilesAsync(IgnoreCommand.kMultipleFilesToIgnore);
      SettingsHandler.SaveGeneralSettings();

      if (!File.Exists(kGeneralSettingsPath))
        Assert.False(true);

      XmlSerializer serializer = new XmlSerializer();
      var generalSettingsModel = serializer.DeserializeFromFile<ClangOptions>(kGeneralSettingsPath);

      var filesToIgnore = generalSettingsModel.FilesToIgnoreCollection;
      var expectedRestul = string.Join(";", IgnoreCommand.kMultipleFilesToIgnore);
      SettingsTestUtility.ResetClangGeneralOptionsView();

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
      SettingsHandler.SaveGeneralSettings();

      expectedResult += ";" + string.Join(";", IgnoreCommand.kMultipleFilesToIgnore);
      var filesToIgnore = mGeneralOptions.FilesToIgnore;
      SettingsTestUtility.ResetClangGeneralOptionsView();

      Assert.Equal(filesToIgnore, expectedResult);
    }


    [VsFact(Version = "2017-")]
    public async System.Threading.Tasks.Task SaveFilesToIgnore_NoEmptyState_ConfigAsync()
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();

      var expectedResult = string.Join(";", IgnoreCommand.kStartUpMultipleFilesToIgnore);
      Initialize(string.Empty);

      await IgnoreFilesAsync(IgnoreCommand.kStartUpMultipleFilesToIgnore);
      await IgnoreFilesAsync(IgnoreCommand.kMultipleFilesToIgnore);
      SettingsHandler.SaveGeneralSettings();

      if (!File.Exists(kGeneralSettingsPath))
        Assert.False(true);

      XmlSerializer serializer = new XmlSerializer();

      var generalSettingsModel = serializer.DeserializeFromFile<ClangOptions>(kGeneralSettingsPath);
      expectedResult += ";" + string.Join(";", IgnoreCommand.kMultipleFilesToIgnore);

      var filesToIgnore = generalSettingsModel.FilesToIgnoreCollection;
      SettingsTestUtility.ResetClangGeneralOptionsView();

      Assert.Equal(filesToIgnore, expectedResult);
    }

    #endregion


    #region Private Methods

    private void Initialize(string ignoreFiles)
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      mDte = (DTE2)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
      SettingsTestUtility.ResetClangGeneralOptionsView();

      mIgnoreCompileCommand = IgnoreCompileCommand.Instance;
      mGeneralOptions = SettingsProvider.GeneralSettings;
      mGeneralOptions.FilesToIgnore = ignoreFiles;
    }

    private System.Threading.Tasks.Task IgnoreFilesAsync(List<string> aFilesToIgnore)
    {
      return System.Threading.Tasks.Task.Run(() =>
      {
        mIgnoreCompileCommand.AddIgnoreFilesToSettings(aFilesToIgnore);
      });
    }

    #endregion

  }
}
