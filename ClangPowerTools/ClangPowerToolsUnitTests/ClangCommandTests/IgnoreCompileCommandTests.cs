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

    [VsFact(Version = "2019")]
    public async System.Threading.Tasks.Task SaveFilesToIgnore_EmptyState_UIAsync()
    {
      //Arrange
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();

      //Act
      Initialize(string.Empty);
      await IgnoreFilesAsync(IgnoreCommand.kSingleFileToIgnore);

      //Assert
      Assert.Equal(mGeneralOptions.FilesToIgnore, string.Join(";", IgnoreCommand.kSingleFileToIgnore));
    }


    [VsFact(Version = "2019")]
    public async System.Threading.Tasks.Task SaveFilesToIgnore_EmptyState_ConfigAsync()
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();

      Initialize(string.Empty);
      await IgnoreFilesAsync(IgnoreCommand.kSingleFileToIgnore);

      if (!File.Exists(kGeneralSettingsPath))
        Assert.False(true);

      XmlSerializer serializer = new XmlSerializer();
      var generalSettingsModel = serializer.DeserializeFromFile<ClangOptions>(kGeneralSettingsPath);

      Assert.Equal(generalSettingsModel.FilesToIgnoreCollection, string.Join(";", IgnoreCommand.kSingleFileToIgnore));
    }


    [VsFact(Version = "2019")]
    public async System.Threading.Tasks.Task SaveMultipleFilesToIgnore_EmptyState_UIAsync()
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();

      Initialize(string.Empty);
      await IgnoreFilesAsync(IgnoreCommand.kMultipleFilesToIgnore);

      Assert.Equal(mGeneralOptions.FilesToIgnore, string.Join(";", IgnoreCommand.kMultipleFilesToIgnore));
    }


    [VsFact(Version = "2019")]
    public async System.Threading.Tasks.Task SaveMultipleFilesToIgnore_EmptyState_ConfigAsync()
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();

      Initialize(string.Empty);
      await IgnoreFilesAsync(IgnoreCommand.kMultipleFilesToIgnore);

      if (!File.Exists(kGeneralSettingsPath))
        Assert.False(true);

      XmlSerializer serializer = new XmlSerializer();
      var generalSettingsModel = serializer.DeserializeFromFile<ClangOptions>(kGeneralSettingsPath);

      Assert.Equal(generalSettingsModel.FilesToIgnoreCollection, string.Join(";", IgnoreCommand.kMultipleFilesToIgnore));
    }


    // No Empty State

    [VsFact(Version = "2019")]
    public async System.Threading.Tasks.Task SaveFilesToIgnore_NoEmptyState_UIAsync()
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();

      var expectedResult = string.Join(";", IgnoreCommand.kStartUpMultipleFilesToIgnore);
      Initialize(expectedResult);
      await IgnoreFilesAsync(IgnoreCommand.kMultipleFilesToIgnore);
      expectedResult += ";" + string.Join(";", IgnoreCommand.kMultipleFilesToIgnore);

      Assert.Equal(mGeneralOptions.FilesToIgnore, expectedResult);
    }


    [VsFact(Version = "2019")]
    public async System.Threading.Tasks.Task SaveFilesToIgnore_NoEmptyState_ConfigAsync()
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();

      var expectedResult = string.Join(";", IgnoreCommand.kStartUpMultipleFilesToIgnore);
      Initialize(string.Empty);

      await IgnoreFilesAsync(IgnoreCommand.kStartUpMultipleFilesToIgnore);
      await IgnoreFilesAsync(IgnoreCommand.kMultipleFilesToIgnore);

      if (!File.Exists(kGeneralSettingsPath))
        Assert.False(true);

      XmlSerializer serializer = new XmlSerializer();

      var generalSettingsModel = serializer.DeserializeFromFile<ClangOptions>(kGeneralSettingsPath);
      expectedResult += ";" + string.Join(";", IgnoreCommand.kMultipleFilesToIgnore);

      Assert.Equal(generalSettingsModel.FilesToIgnoreCollection, expectedResult);
    }

    #endregion


    #region Private Methods

    private void Initialize(string ignoreFiles)
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      mDte = (DTE2)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
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
