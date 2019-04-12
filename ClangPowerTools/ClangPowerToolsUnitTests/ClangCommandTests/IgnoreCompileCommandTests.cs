using System.Collections.Generic;
using System.IO;
using ClangPowerTools.Commands;
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

    private const string generalSettingsPath = @"C:\Users\Enache Ionut\AppData\Roaming\ClangPowerTools\GeneralConfiguration.config";

    private List<string> mFileToIgnore = new List<string>()
    {
      @"DispatcherHandler.cpp",
    };

    private List<string> mMultipleFilesToIgnore = new List<string>()
    {
      @"DispatcherHandler.cpp",
      @"VsServiceProviderTests.cpp",
      @"AsyncPackageTests.cpp"
    };

    private List<string> mInitialMultipleFilesToIgnore = new List<string>()
    {
      @"CommandController.cpp",
      @"Settings.cpp",
      @"Options.cpp"
    };

    private DTE2 mDte;
    private IgnoreCompileCommand mIgnoreCompileCommand;
    private ClangGeneralOptionsView mGeneralOptions;

    #endregion

    #region Methods

    #region Public Methods

    // Empty State 

    [VsFact(Version = "2019")]
    public async System.Threading.Tasks.Task SaveFilesToIgnore_EmptyState_UIAsync()
    {
      await UnitTestUtility.LoadPackageAsync();
      Initialize(string.Empty);

      await IgnoreFiles(mFileToIgnore);

      Assert.Equal(mGeneralOptions.FilesToIgnore, string.Join(";", mFileToIgnore));
    }


    [VsFact(Version = "2019")]
    public async System.Threading.Tasks.Task SaveFilesToIgnore_EmptyState_ConfigAsync()
    {
      await UnitTestUtility.LoadPackageAsync();
      Initialize(string.Empty);

      await IgnoreFiles(mFileToIgnore);

      if (!File.Exists(generalSettingsPath))
        Assert.False(true);

      XmlSerializer serializer = new XmlSerializer();
      var generalSettingsModel = serializer.DeserializeFromFile<ClangOptions>(generalSettingsPath);

      Assert.Equal(generalSettingsModel.FilesToIgnoreCollection, string.Join(";", mFileToIgnore));
    }


    [VsFact(Version = "2019")]
    public async System.Threading.Tasks.Task SaveMultipleFilesToIgnore_EmptyState_UIAsync()
    {
      await UnitTestUtility.LoadPackageAsync();
      Initialize(string.Empty);

      await IgnoreFiles(mMultipleFilesToIgnore);

      Assert.Equal(mGeneralOptions.FilesToIgnore, string.Join(";", mMultipleFilesToIgnore));
    }


    [VsFact(Version = "2019")]
    public async System.Threading.Tasks.Task SaveMultipleFilesToIgnore_EmptyState_ConfigAsync()
    {
      await UnitTestUtility.LoadPackageAsync();
      Initialize(string.Empty);

      await IgnoreFiles(mMultipleFilesToIgnore);

      if (!File.Exists(generalSettingsPath))
        Assert.False(true);

      XmlSerializer serializer = new XmlSerializer();
      var generalSettingsModel = serializer.DeserializeFromFile<ClangOptions>(generalSettingsPath);

      Assert.Equal(generalSettingsModel.FilesToIgnoreCollection, string.Join(";", mMultipleFilesToIgnore));
    }


    // No Empty State

    [VsFact(Version = "2019")]
    public async System.Threading.Tasks.Task SaveFilesToIgnore_NoEmptyState_UIAsync()
    {
      await UnitTestUtility.LoadPackageAsync();

      var expectedResult = string.Join(";", mInitialMultipleFilesToIgnore);
      Initialize(expectedResult);

      await IgnoreFiles(mMultipleFilesToIgnore);

      expectedResult += ";" + string.Join(";", mMultipleFilesToIgnore);
      Assert.Equal(mGeneralOptions.FilesToIgnore, expectedResult);
    }


    [VsFact(Version = "2019")]
    public async System.Threading.Tasks.Task SaveFilesToIgnore_NoEmptyState_ConfigAsync()
    {
      await UnitTestUtility.LoadPackageAsync();

      var expectedResult = string.Join(";", mInitialMultipleFilesToIgnore);
      Initialize(string.Empty);

      await IgnoreFiles(mInitialMultipleFilesToIgnore);
      await IgnoreFiles(mMultipleFilesToIgnore);

      if (!File.Exists(generalSettingsPath))
        Assert.False(true);

      XmlSerializer serializer = new XmlSerializer();

      var generalSettingsModel = serializer.DeserializeFromFile<ClangOptions>(generalSettingsPath);
      expectedResult += ";" + string.Join(";", mMultipleFilesToIgnore);

      Assert.Equal(generalSettingsModel.FilesToIgnoreCollection, expectedResult);
    }

    #endregion


    #region Private Methods

    private void Initialize(string ignoreFiles)
    {
      mDte = (DTE2)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
      mIgnoreCompileCommand = IgnoreCompileCommand.Instance;
      mGeneralOptions = SettingsProvider.GeneralSettings;
      mGeneralOptions.FilesToIgnore = ignoreFiles;
    }

    private async System.Threading.Tasks.Task IgnoreFiles(List<string> aFilesToIgnore)
    {
      await System.Threading.Tasks.Task.Run(() =>
      {
        mIgnoreCompileCommand.AddIgnoreFilesToSettings(aFilesToIgnore);
      });
    }

    #endregion


    #endregion

  }
}
