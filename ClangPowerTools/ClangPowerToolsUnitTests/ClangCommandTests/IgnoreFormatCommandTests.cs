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

    [VsFact(Version = "2019")]
    public async System.Threading.Tasks.Task SaveFilesToIgnore_EmptyState_UIAsync()
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

      await UnitTestUtility.LoadPackageAsync();
      Initialize(string.Empty);

      await IgnoreFiles(IgnoreCommand.kSingleFileToIgnore);

      Assert.Equal(mFormatOptions.FilesToIgnore, string.Join(";", IgnoreCommand.kSingleFileToIgnore));
    }


    [VsFact(Version = "2019")]
    public async System.Threading.Tasks.Task SaveFilesToIgnore_EmptyState_ConfigAsync()
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

      await UnitTestUtility.LoadPackageAsync();
      Initialize(string.Empty);

      await IgnoreFiles(IgnoreCommand.kSingleFileToIgnore);

      if (!File.Exists(kFormatSettingsPath))
        Assert.False(true);

      XmlSerializer serializer = new XmlSerializer();
      var generalSettingsModel = serializer.DeserializeFromFile<ClangFormatOptions>(kFormatSettingsPath);

      Assert.Equal(generalSettingsModel.SkipFiles, string.Join(";", IgnoreCommand.kSingleFileToIgnore));
    }


    [VsFact(Version = "2019")]
    public async System.Threading.Tasks.Task SaveMultipleFilesToIgnore_EmptyState_UIAsync()
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

      await UnitTestUtility.LoadPackageAsync();
      Initialize(string.Empty);

      await IgnoreFiles(IgnoreCommand.kMultipleFilesToIgnore);

      Assert.Equal(mFormatOptions.FilesToIgnore, string.Join(";", IgnoreCommand.kMultipleFilesToIgnore));
    }


    [VsFact(Version = "2019")]
    public async System.Threading.Tasks.Task SaveMultipleFilesToIgnore_EmptyState_ConfigAsync()
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

      await UnitTestUtility.LoadPackageAsync();
      Initialize(string.Empty);

      await IgnoreFiles(IgnoreCommand.kMultipleFilesToIgnore);

      if (!File.Exists(kFormatSettingsPath))
        Assert.False(true);

      XmlSerializer serializer = new XmlSerializer();
      var generalSettingsModel = serializer.DeserializeFromFile<ClangFormatOptions>(kFormatSettingsPath);

      Assert.Equal(generalSettingsModel.SkipFiles, string.Join(";", IgnoreCommand.kMultipleFilesToIgnore));
    }


    // No Empty State

    [VsFact(Version = "2019")]
    public async System.Threading.Tasks.Task SaveFilesToIgnore_NoEmptyState_UIAsync()
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

      await UnitTestUtility.LoadPackageAsync();

      var expectedResult = string.Join(";", IgnoreCommand.kStartUpMultipleFilesToIgnore);
      Initialize(expectedResult);

      await IgnoreFiles(IgnoreCommand.kMultipleFilesToIgnore);

      expectedResult += ";" + string.Join(";", IgnoreCommand.kMultipleFilesToIgnore);
      Assert.Equal(mFormatOptions.FilesToIgnore, expectedResult);
    }


    [VsFact(Version = "2019")]
    public async System.Threading.Tasks.Task SaveFilesToIgnore_NoEmptyState_ConfigAsync()
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

      await UnitTestUtility.LoadPackageAsync();

      var expectedResult = string.Join(";", IgnoreCommand.kStartUpMultipleFilesToIgnore);
      Initialize(expectedResult);

      //await IgnoreFiles(mInitialMultipleFilesToIgnore);
      await IgnoreFiles(IgnoreCommand.kMultipleFilesToIgnore);

      if (!File.Exists(kFormatSettingsPath))
        Assert.False(true);

      XmlSerializer serializer = new XmlSerializer();

      var generalSettingsModel = serializer.DeserializeFromFile<ClangFormatOptions>(kFormatSettingsPath);
      expectedResult += ";" + string.Join(";", IgnoreCommand.kMultipleFilesToIgnore);

      Assert.Equal(generalSettingsModel.SkipFiles, expectedResult);
    }

    #endregion


    #region Private Methods

    private void Initialize(string ignoreFiles)
    {
      mDte = (DTE2)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
      mIgnoreFormatCommand = IgnoreFormatCommand.Instance;
      mFormatOptions = SettingsProvider.ClangFormatSettings;
      mFormatOptions.FilesToIgnore = ignoreFiles;
    }

    private async System.Threading.Tasks.Task IgnoreFiles(List<string> aFilesToIgnore)
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
