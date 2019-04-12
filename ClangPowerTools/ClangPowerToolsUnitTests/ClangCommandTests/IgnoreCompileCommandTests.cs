using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    private List<string> mFirstFileToIgnore = new List<string>()
    {
      @"DispatcherHandler.cpp",
    };

    private List<string> mFirstMultipleFilesToIgnore = new List<string>()
    {
      @"DispatcherHandler.cpp",
      @"VsServiceProviderTests.cs",
      @"AsyncPackageTests.cs"
    };


    #endregion

    #region Methods

    #region Public Methods



    [VsFact(Version = "2019")]
    public async System.Threading.Tasks.Task SaveFirstFilesToIgnore_UIAsync()
    {
      await UnitTestUtility.LoadPackageAsync();

      var dte = (DTE2)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
      var ignoreCompileCommand = IgnoreCompileCommand.Instance;
      var generalSettings = SettingsProvider.GeneralSettings;

      generalSettings.FilesToIgnore = string.Empty;

      await System.Threading.Tasks.Task.Run(() =>
      {
        ignoreCompileCommand.AddIgnoreFilesToSettings(mFirstFileToIgnore);
      });

      var settingsResult = generalSettings.FilesToIgnore;
      var expectedResult = string.Join(";", mFirstFileToIgnore);

      Assert.True(settingsResult == expectedResult);
    }


    [VsFact(Version = "2019")]
    public async System.Threading.Tasks.Task SaveFirstFilesToIgnore_ConfigFileAsync()
    {
      await UnitTestUtility.LoadPackageAsync();

      var dte = (DTE2)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
      var ignoreCompileCommand = IgnoreCompileCommand.Instance;
      var generalSettings = SettingsProvider.GeneralSettings;

      generalSettings.FilesToIgnore = string.Empty;

      await System.Threading.Tasks.Task.Run(() =>
      {
        ignoreCompileCommand.AddIgnoreFilesToSettings(mFirstFileToIgnore);
      });

      if (!File.Exists(generalSettingsPath))
        Assert.False(true);

      XmlSerializer serializer = new XmlSerializer();

      var generalSettingsModel = serializer.DeserializeFromFile<ClangOptions>(generalSettingsPath);
      var expectedResult = string.Join(";", mFirstFileToIgnore);
      var settingsResult = generalSettingsModel.FilesToIgnoreCollection;

      Assert.True(settingsResult == expectedResult);
    }


    [VsFact(Version = "2019")]
    public async System.Threading.Tasks.Task SaveFirstMultipleFilesToIgnore_UIAsync()
    {
      await UnitTestUtility.LoadPackageAsync();

      var dte = (DTE2)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
      var ignoreCompileCommand = IgnoreCompileCommand.Instance;
      var generalSettings = SettingsProvider.GeneralSettings;

      generalSettings.FilesToIgnore = string.Empty;

      await System.Threading.Tasks.Task.Run(() =>
      {
        ignoreCompileCommand.AddIgnoreFilesToSettings(mFirstMultipleFilesToIgnore);
      });

      var settingsResult = generalSettings.FilesToIgnore;
      var expectedResult = string.Join(";", mFirstMultipleFilesToIgnore);

      Assert.True(settingsResult == expectedResult);
    }


    [VsFact(Version = "2019")]
    public async System.Threading.Tasks.Task SaveFirstMultipleFilesToIgnore_ConfigFileAsync()
    {
      await UnitTestUtility.LoadPackageAsync();

      var dte = (DTE2)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
      var ignoreCompileCommand = IgnoreCompileCommand.Instance;
      var generalSettings = SettingsProvider.GeneralSettings;

      generalSettings.FilesToIgnore = string.Empty;

      await System.Threading.Tasks.Task.Run(() =>
      {
        ignoreCompileCommand.AddIgnoreFilesToSettings(mFirstMultipleFilesToIgnore);
      });

      if (!File.Exists(generalSettingsPath))
        Assert.False(true);

      XmlSerializer serializer = new XmlSerializer();

      var generalSettingsModel = serializer.DeserializeFromFile<ClangOptions>(generalSettingsPath);
      var expectedResult = string.Join(";", mFirstMultipleFilesToIgnore);
      var settingsResult = generalSettingsModel.FilesToIgnoreCollection;

      Assert.True(settingsResult == expectedResult);
    }



    //[VsFact(Version = "2019")]
    //public async System.Threading.Tasks.Task SaveFirstFilesToIgnore_ConfigFileAsync()
    //{
    //  await UnitTestUtility.LoadPackageAsync();

    //  var dte = (DTE2)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
    //  var ignoreCompileCommand = IgnoreCompileCommand.Instance;
    //  var generalSettings = SettingsProvider.GeneralSettings;

    //  generalSettings.FilesToIgnore = string.Empty;

    //  await System.Threading.Tasks.Task.Run(() =>
    //  {
    //    ignoreCompileCommand.AddIgnoreFilesToSettings(mFirstFileToIgnore);
    //  });

    //  if (!File.Exists(generalSettingsPath))
    //    Assert.False(true);

    //  XmlSerializer serializer = new XmlSerializer();

    //  var generalSettingsModel = serializer.DeserializeFromFile<ClangOptions>(generalSettingsPath);
    //  var expectedResult = string.Join(";", mFirstFileToIgnore);
    //  var settingsResult = generalSettingsModel.FilesToIgnoreCollection;

    //  Assert.True(settingsResult == expectedResult);
    //}

    #endregion

    #region Private Methods


    #endregion


    #endregion

  }
}
