using ClangPowerTools.Builder;
using ClangPowerTools.Convertors;
using ClangPowerTools.Services;
using EnvDTE80;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace ClangPowerTools.Script
{
  /// <summary>
  /// Contains all the script creation logic and parameters checking for the generic parameters components(environment and settings) 
  /// The result will be a string which represents the way how the power shell script will be called
  /// </summary>
  public class GenericScriptBuilder : IBuilder<string>
  {
    #region Members

    /// <summary>
    /// The final result after the build method
    /// </summary>
    private string mScript = string.Empty;

    private string mVsEdition;
    private string mVsVersion;
    private int mCommandId;
    private bool jsonCompilationDbActive;

    #endregion


    #region Constructor

    /// <summary>
    /// Instance constructor
    /// </summary>
    public GenericScriptBuilder(string aVsEdition, string aVsVersion, int aCommandId, bool jsonCompilation = false)
    {
      mVsEdition = aVsEdition;
      mVsVersion = aVsVersion;
      mCommandId = aCommandId;
      jsonCompilationDbActive = jsonCompilation;
    }

    #endregion


    #region Methods

    #region Public Methods

    #region IBuilder Implementation


    /// <summary>
    /// Create the generic script by gathering all the generic parameters from the environment and settings components 
    /// </summary>
    public void Build()
    {
      // Append the General parameters and Tidy parameters from option pages
      mScript = $"{GetGeneralParameters()} {(CommandIds.kTidyId == mCommandId || CommandIds.kTidyFixId == mCommandId ? GetTidyParameters() : ScriptConstants.kParallel)}";

      var formatSettings = SettingsProvider.FormatSettingsModel;
      var tidySettings = SettingsProvider.TidySettingsModel;

      // Append the clang-format style
      if (null != formatSettings && null != tidySettings && CommandIds.kTidyFixId == mCommandId && tidySettings.FormatAfterTidy)
        mScript += $" {ScriptConstants.kClangFormatStyle} {formatSettings.Style}";

      // Append the Visual Studio Version and Edition
      mScript += $" {ScriptConstants.kVsVersion} {mVsVersion}";

      // Append the solution path
      if (VsServiceProvider.TryGetService(typeof(DTE2), out object dte))
        mScript += $" {ScriptConstants.kDirectory} '{(dte as DTE2).Solution.FullName}' ";
    }


    /// <summary>
    /// Get the script after the build process
    /// </summary>
    /// <returns>The script which will contain the generic parameters</returns>
    public string GetResult() => mScript;


    #endregion


    #endregion


    #region Private Methods

    /// <summary>
    /// Get the parameters from the General option page
    /// </summary>
    /// <param name="mGeneralOptions"></param>
    /// <returns>The parameters from General option page</returns>
    private string GetGeneralParameters()
    {
      var compilerSettings = SettingsProvider.CompilerSettingsModel;
      var parameters = new StringBuilder();

      // Get the Clang Flags list
      if (!string.IsNullOrWhiteSpace(compilerSettings.CompileFlags))
        parameters.Append(GetClangFlags());

      // Get the continue when errors are detected flag 
      if (compilerSettings.ContinueOnError)
        parameters.Append($" {ScriptConstants.kContinue}");

      // Get the verbose level flags
      VerbosityScriptBuilder verbosityScriptBuilder = new VerbosityScriptBuilder(compilerSettings.VerbosityLevel);
      verbosityScriptBuilder.Build();
      parameters.Append(verbosityScriptBuilder.GetResult());

      // Get the projects to ignore list 
      if (!string.IsNullOrWhiteSpace(compilerSettings.ProjectsToIgnore))
        parameters.Append($" {ScriptConstants.kProjectsToIgnore} ('{TransformInPowerShellArray(compilerSettings.ProjectsToIgnore)}')");

      // Get the files to ignore list
      if (!string.IsNullOrWhiteSpace(compilerSettings.FilesToIgnore))
        parameters.Append($" {ScriptConstants.kFilesToIgnore} ('{TransformInPowerShellArray(compilerSettings.FilesToIgnore)}')");

      // Get the selected Additional Includes type  
      if (0 == string.Compare(ClangGeneralAdditionalIncludesConvertor.ToString(compilerSettings.AdditionalIncludes), ComboBoxConstants.kSystemIncludeDirectories))
        parameters.Append($" {ScriptConstants.kSystemIncludeDirectories}");

      if (jsonCompilationDbActive)
        parameters.Append($" {ScriptConstants.kJsonCompilationDb}");

      return parameters.ToString();
    }


    /// <summary>
    /// Get the clang flags in the power shell script format
    /// </summary>
    /// <returns>The clang flags</returns>
    private string GetClangFlags()
    {
      var compilerSettings = SettingsProvider.CompilerSettingsModel;

      return string.Format("{0} {1}", ScriptConstants.kClangFlags,
        compilerSettings.WarningsAsErrors ?
          $" ('{ScriptConstants.kTreatWarningsAsErrors}','{TransformInPowerShellArray(compilerSettings.CompileFlags)}')" :
          $" ('{TransformInPowerShellArray(compilerSettings.CompileFlags)}')");
    }


    /// <summary>
    /// Transform the UI parameters list in an power shell array 
    /// </summary>
    /// <param name="aParametersList">The list of UI parameters</param>
    /// <returns>The power shell array containing the UI parameters</returns>
    private string TransformInPowerShellArray(string aParametersList) =>
      string.Join("','", aParametersList.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));


    /// <summary>
    /// Get the parameters from the Tidy related option page
    /// </summary>
    /// <returns></returns>
    private string GetTidyParameters()
    {
      var tidySettings = SettingsProvider.TidySettingsModel;

      // Get the clang tidy parameters depending on the tidy mode
      var parameters = GetTidyChecks(tidySettings);

      // Append the clang tidy type(tidy / tidy-fix) with / without clang tidy config file option attached  
      if (!string.IsNullOrWhiteSpace(parameters))
      {
        if (tidySettings.UseChecksFrom != ClangTidyUseChecksFrom.TidyFile)
        {
          if (parameters.Count() == 3)
            parameters = parameters.Remove(2, 1);
          var filePath = Path.Combine(Path.GetTempPath(), ".clang-tidy");
          var text = $"Checks: '{parameters}'";

          using FileStream fs = new FileStream(filePath, FileMode.Create);
          using StreamWriter sw = new StreamWriter(fs);
          sw.Write(text);
          parameters = AppendClangTidyType(filePath);
        }
        else
        {
          parameters = AppendClangTidyType(parameters);
        }
      }

      // Get the header filter option 
      if (null != tidySettings.HeaderFilter && !string.IsNullOrWhiteSpace(tidySettings.HeaderFilter))
        parameters += $" {GetHeaderFilters()}";

      // Get the compilation database option
      if (null != tidySettings.CompilationDatabaseDir && !string.IsNullOrWhiteSpace(tidySettings.CompilationDatabaseDir))
        parameters += $" {GetCompilationDatabaseDir(tidySettings.CompilationDatabaseDir)}";

      parameters += $" {ScriptConstants.kParallel}";
      return parameters;
    }


    /// <summary>
    /// Append the clang tidy type(tidy / tidy-fix) with/ without tidy config file option attached
    /// </summary>
    /// <param name="aParameters"></param>
    /// <returns>The <"aParameters"> value with the clang tidy type with / without the clang tidy config file option attached</returns>
    private string AppendClangTidyType(string aParameters)
    {
      return string.Format("{0} '{1}'",
        (CommandIds.kTidyFixId == mCommandId ? ScriptConstants.kTidyFix : ScriptConstants.kTidy),
        aParameters);
    }


    /// <summary>
    /// Get the header filter option from the Clang Tidy Option page
    /// </summary>
    /// <returns>Header filter option</returns>
    private string GetHeaderFilters()
    {
      var tidySettings = SettingsProvider.TidySettingsModel;

      return string.Format("{0} '{1}'", ScriptConstants.kHeaderFilter,
        string.IsNullOrWhiteSpace(ClangTidyHeaderFiltersConvertor.ScriptEncode(tidySettings.HeaderFilter)) ?
           tidySettings.HeaderFilter :
           ClangTidyHeaderFiltersConvertor.ScriptEncode(tidySettings.HeaderFilter));
    }

    private string GetTidyChecks(TidySettingsModel tidyModel)
    {
      ClangTidyUseChecksFrom useChecksFrom = tidyModel.UseChecksFrom;

      if (useChecksFrom == ClangTidyUseChecksFrom.CustomChecks)
      {
        return ScriptConstants.kTidyCheckFirstElement + tidyModel.CustomChecks.Replace(';', ',').TrimEnd(',');
      }
      else if (useChecksFrom == ClangTidyUseChecksFrom.PredefinedChecks)
      {
        return ScriptConstants.kTidyCheckFirstElement + tidyModel.PredefinedChecks.Replace(';', ',').TrimEnd(',');
      }
      else
      {
        return ScriptConstants.kTidyFile;
      }
    }

    /// <summary>
    /// Get the compilation database directory option from the Clang Tidy Option page
    /// </summary>
    /// <returns>Compilation database directory option</returns>
    private string GetCompilationDatabaseDir(string compilationDatabaseDir)
    {
        return string.Format("{0} '{1}'", ScriptConstants.kCompilationDatabaseDir, compilationDatabaseDir);
    }
    #endregion


    #endregion

  }
}
