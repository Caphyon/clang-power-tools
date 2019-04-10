using Xunit;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;
using ClangPowerTools.DialogPages;
using System;
using ClangPowerTools.Services;
using EnvDTE;
using EnvDTE80;

namespace ClangPowerTools.Tests.Settings
{
  [VsTestSettings(UIThread = true)]
  public class GeneralSettingsTests
  {
    [VsFact(Version = "2019")]
    public async Task ClangGeneralOptionsView_NotNullAsync()
    {
      //Arrange
      await UnitTestUtility.LoadPackageAsync();

      //Act
      ClangGeneralOptionsView generalSettings = SettingsProvider.GeneralSettings;

      //Assert
      Assert.NotNull(generalSettings);
    }

    [VsFact(Version = "2019")]
    public async Task CompileFlags_CompareViewToFileAsync()
    {
      //Arrange
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();
      ClangGeneralOptionsView generalSettings = SettingsProvider.GeneralSettings;

      //Act
      ClangGeneralOptionsView generalSettingsFromFile = UnitTestUtility.GetClangGeneralOptionsViewFromFile();

      //Assert
      Assert.Equal(generalSettings.ClangCompileAfterVsCompile, generalSettingsFromFile.ClangCompileAfterVsCompile);
    }

    [VsFact(Version = "2019")]
    public async Task FilesToIgnore_CompareViewToFileAsync()
    {
      //Arrange
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();
      ClangGeneralOptionsView generalSettings = SettingsProvider.GeneralSettings;

      //Act
      ClangGeneralOptionsView generalSettingsFromFile = UnitTestUtility.GetClangGeneralOptionsViewFromFile();

      //Assert
      Assert.Equal(generalSettings.FilesToIgnore, generalSettingsFromFile.FilesToIgnore);
    }

    [VsFact(Version = "2019")]
    public async Task ProjectToIgnore_CompareViewToFileAsync()
    {
      //Arrange
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();
      ClangGeneralOptionsView generalSettings = SettingsProvider.GeneralSettings;

      //Act
      ClangGeneralOptionsView generalSettingsFromFile = UnitTestUtility.GetClangGeneralOptionsViewFromFile();

      //Assert
      Assert.Equal(generalSettings.ProjectsToIgnore, generalSettingsFromFile.ProjectsToIgnore);
    }

    [VsFact(Version = "2019")]
    public async Task AdditionalIncludes_CompareViewToFileAsync()
    {
      //Arrange
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();
      ClangGeneralOptionsView generalSettings = SettingsProvider.GeneralSettings;

      //Act
      ClangGeneralOptionsView generalSettingsFromFile = UnitTestUtility.GetClangGeneralOptionsViewFromFile();

      //Assert
      Assert.Equal(generalSettings.AdditionalIncludes, generalSettingsFromFile.AdditionalIncludes);
    }

    [VsFact(Version = "2019")]
    public async Task TreatWarningsAsErrors_CompareViewToFileAsync()
    {
      //Arrange
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();
      ClangGeneralOptionsView generalSettings = SettingsProvider.GeneralSettings;

      //Act
      ClangGeneralOptionsView generalSettingsFromFile = UnitTestUtility.GetClangGeneralOptionsViewFromFile();

      //Assert
      Assert.Equal(generalSettings.TreatWarningsAsErrors, generalSettingsFromFile.TreatWarningsAsErrors);
    }

    [VsFact(Version = "2019")]
    public async Task ContinueOnError_CompareViewToFileAsync()
    {
      //Arrange
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();
      ClangGeneralOptionsView generalSettings = SettingsProvider.GeneralSettings;

      //Act
      ClangGeneralOptionsView generalSettingsFromFile = UnitTestUtility.GetClangGeneralOptionsViewFromFile();

      //Assert
      Assert.Equal(generalSettings.Continue, generalSettingsFromFile.Continue);
    }

    [VsFact(Version = "2019")]
    public async Task ClangCompileAfterVsCompile_CompareViewToFileAsync()
    {
      //Arrange
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();
      ClangGeneralOptionsView generalSettings = SettingsProvider.GeneralSettings;

      //Act
      ClangGeneralOptionsView generalSettingsFromFile = UnitTestUtility.GetClangGeneralOptionsViewFromFile();

      //Assert
      Assert.Equal(generalSettings.ClangCompileAfterVsCompile, generalSettingsFromFile.ClangCompileAfterVsCompile);
    }

    [VsFact(Version = "2019")]
    public async Task VerboseMode_CompareViewToFileAsync()
    {
      //Arrange
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();
      ClangGeneralOptionsView generalSettings = SettingsProvider.GeneralSettings;

      //Act
      ClangGeneralOptionsView generalSettingsFromFile = UnitTestUtility.GetClangGeneralOptionsViewFromFile();

      //Assert
      Assert.Equal(generalSettings.VerboseMode, generalSettingsFromFile.VerboseMode);
    }

    [VsFact(Version = "2019")]
    public async Task FilesToIgnore_ChangeValue_CompareViewToFileAsync()
    {
      //Arrange
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();
      VsServiceProvider.TryGetService(typeof(DTE), out object dteService);
      var dte = dteService as DTE;
      ClangGeneralOptionsView generalSettings = SettingsProvider.GeneralSettings;
      Random rand = new Random();
      Commands2 commands = dte.Commands as Commands2;
      Command command;

      //Act
      if (UnitTestUtility.GetCommandByID(commands, "498fdff5-5217-4da9-88d2-edad44ba3874", CommandIds.kSettingsId, out command))
      {
        dte.ExecuteCommand(command.Name);
      }

      generalSettings.FilesToIgnore = rand.Next(1, 5).ToString();
      ClangGeneralOptionsView generalSettingsFromFile = UnitTestUtility.GetClangGeneralOptionsViewFromFile();

      //Assert
      Assert.Equal(generalSettings.FilesToIgnore, generalSettingsFromFile.FilesToIgnore);
    }
  }
}
