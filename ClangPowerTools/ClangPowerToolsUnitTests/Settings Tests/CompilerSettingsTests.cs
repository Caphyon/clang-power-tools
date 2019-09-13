using Xunit;

namespace ClangPowerTools.Tests.Settings
{
  [VsTestSettings(UIThread = true)]
  public class CompilerSettingsTests
  {
    [VsFact(Version = "2019-")]
    public void CompilerSettings_NotNull()
    {
      //Arrange
      SettingsProvider settingsProvider = new SettingsProvider();

      //Act
      CompilerSettingsModel compilerSettingsModel = settingsProvider.GetCompilerSettingsModel();

      //Assert
      Assert.NotNull(compilerSettingsModel);
    }

    [VsFact(Version = "2019-")]
    public void CompileFlags_ChangeValue_CompareViewToFile()
    {
      SettingsHandler settingsHandler = new SettingsHandler();
      SettingsProvider settingsProvider = new SettingsProvider();
      CompilerSettingsModel compilerSettingsModel = new CompilerSettingsModel
      {
        CompileFlags = "-Wall"
      };

      settingsProvider.SetCompilerSettingsModel(compilerSettingsModel);
      settingsHandler.SaveSettings();
      settingsHandler.ResetSettings();
      settingsHandler.LoadSettings();

      Assert.Equal(compilerSettingsModel.CompileFlags, settingsProvider.GetCompilerSettingsModel().CompileFlags);
    }

    [VsFact(Version = "2019-")]
    public void FilesToIgnore_ChangeValue_CompareViewToFile()
    {
      SettingsHandler settingsHandler = new SettingsHandler();
      SettingsProvider settingsProvider = new SettingsProvider();
      CompilerSettingsModel compilerSettingsModel = new CompilerSettingsModel
      {
        FilesToIgnore = "Test.cpp"
      };

      settingsProvider.SetCompilerSettingsModel(compilerSettingsModel);
      settingsHandler.SaveSettings();
      settingsHandler.ResetSettings();
      settingsHandler.LoadSettings();

      Assert.Equal(compilerSettingsModel.FilesToIgnore, settingsProvider.GetCompilerSettingsModel().FilesToIgnore);
    }

    [VsFact(Version = "2019-")]
    public void ProjectToIgnore_ChangeValue_CompareViewToFile()
    {
      SettingsHandler settingsHandler = new SettingsHandler();
      SettingsProvider settingsProvider = new SettingsProvider();
      CompilerSettingsModel compilerSettingsModel = new CompilerSettingsModel
      {
        ProjectsToIgnore = "TestProject"
      };

      settingsProvider.SetCompilerSettingsModel(compilerSettingsModel);
      settingsHandler.SaveSettings();
      settingsHandler.ResetSettings();
      settingsHandler.LoadSettings();

      Assert.Equal(compilerSettingsModel.ProjectsToIgnore, settingsProvider.GetCompilerSettingsModel().ProjectsToIgnore);
    }

    [VsFact(Version = "2019-")]
    public void AdditionalIncludes_ChangeValue_CompareViewToFile()
    {
      SettingsHandler settingsHandler = new SettingsHandler();
      SettingsProvider settingsProvider = new SettingsProvider();
      CompilerSettingsModel compilerSettingsModel = new CompilerSettingsModel
      {
        AdditionalIncludes = ClangGeneralAdditionalIncludes.SystemIncludeDirectories
      };

      settingsProvider.SetCompilerSettingsModel(compilerSettingsModel);
      settingsHandler.SaveSettings();
      settingsHandler.ResetSettings();
      settingsHandler.LoadSettings();

      Assert.Equal(compilerSettingsModel.AdditionalIncludes, settingsProvider.GetCompilerSettingsModel().AdditionalIncludes);
    }

    [VsFact(Version = "2019-")]
    public void TreatWarningsAsErrors_ChangeValue_CompareViewToFile()
    {
      SettingsHandler settingsHandler = new SettingsHandler();
      SettingsProvider settingsProvider = new SettingsProvider();
      CompilerSettingsModel compilerSettingsModel = new CompilerSettingsModel
      {
        WarningsAsErrors = true
      };

      settingsProvider.SetCompilerSettingsModel(compilerSettingsModel);
      settingsHandler.SaveSettings();
      settingsHandler.ResetSettings();
      settingsHandler.LoadSettings();

      Assert.Equal(compilerSettingsModel.WarningsAsErrors, settingsProvider.GetCompilerSettingsModel().WarningsAsErrors);
    }

    [VsFact(Version = "2019-")]
    public void ContinueOnError_ChangeValue_CompareViewToFile()
    {
      SettingsHandler settingsHandler = new SettingsHandler();
      SettingsProvider settingsProvider = new SettingsProvider();
      CompilerSettingsModel compilerSettingsModel = new CompilerSettingsModel
      {
        ContinueOnError = true
      };

      settingsProvider.SetCompilerSettingsModel(compilerSettingsModel);
      settingsHandler.SaveSettings();
      settingsHandler.ResetSettings();
      settingsHandler.LoadSettings();

      Assert.Equal(compilerSettingsModel.ContinueOnError, settingsProvider.GetCompilerSettingsModel().ContinueOnError);
    }

    [VsFact(Version = "2019-")]
    public void ClangCompileAfterVsCompile_ChangeValue_CompareViewToFile()
    {
      SettingsHandler settingsHandler = new SettingsHandler();
      SettingsProvider settingsProvider = new SettingsProvider();
      CompilerSettingsModel compilerSettingsModel = new CompilerSettingsModel
      {
        ClangAfterMSVC = true
      };

      settingsProvider.SetCompilerSettingsModel(compilerSettingsModel);
      settingsHandler.SaveSettings();
      settingsHandler.ResetSettings();
      settingsHandler.LoadSettings();

      Assert.Equal(compilerSettingsModel.ClangAfterMSVC, settingsProvider.GetCompilerSettingsModel().ClangAfterMSVC);
    }

    [VsFact(Version = "2019-")]
    public void VerboseMode_ChangeValue_CompareViewToFile()
    {
      SettingsHandler settingsHandler = new SettingsHandler();
      SettingsProvider settingsProvider = new SettingsProvider();
      CompilerSettingsModel compilerSettingsModel = new CompilerSettingsModel
      {
        VerboseMode = true
      };

      settingsProvider.SetCompilerSettingsModel(compilerSettingsModel);
      settingsHandler.SaveSettings();
      settingsHandler.ResetSettings();
      settingsHandler.LoadSettings();

      Assert.Equal(compilerSettingsModel.VerboseMode, settingsProvider.GetCompilerSettingsModel().VerboseMode);
    }
  }
}
