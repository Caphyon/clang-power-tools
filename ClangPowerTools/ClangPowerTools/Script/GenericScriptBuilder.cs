using ClangPowerTools.Builder;
using ClangPowerTools.Convertors;
using ClangPowerTools.DialogPages;
using ClangPowerTools.Services;
using EnvDTE;
using EnvDTE80;
using System;

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
    private bool mTidyFixFlag;
    private bool mUseClangTidyConfigFile;


    #endregion


    #region Constructor

    /// <summary>
    /// Instance constructor
    /// </summary>
    public GenericScriptBuilder(string aVsEdition, string aVsVersion, int aCommandId, bool aTidyFixFlag = false)
    {
      mVsEdition = aVsEdition;
      mVsVersion = aVsVersion;
      mCommandId = aCommandId;
      mTidyFixFlag = aTidyFixFlag;
      mUseClangTidyConfigFile = false;
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

      var clangFormatSettings = SettingsProvider.GetSettingsPage(typeof(ClangFormatOptionsView)) as ClangFormatOptionsView;
      var tidySettings = SettingsProvider.GetSettingsPage(typeof(ClangTidyOptionsView)) as ClangTidyOptionsView;

      // Append the clang-format style
      if (null != clangFormatSettings && null != tidySettings && mTidyFixFlag && tidySettings.FormatAfterTidy)
        mScript += $" {ScriptConstants.kClangFormatStyle} {clangFormatSettings.Style}";

      // Append the Visual Studio Version and Edition
      mScript += $" {ScriptConstants.kVsVersion} {mVsVersion} {ScriptConstants.kVsEdition} {mVsEdition}";

      // Append the solution path
      if (VsServiceProvider.TryGetService(typeof(DTE), out object dte))
        mScript += $" {ScriptConstants.kDirectory} ''{(dte as DTE2).Solution.FullName}''";
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
      var generalSettings = SettingsProvider.GetSettingsPage(typeof(ClangGeneralOptionsView)) as ClangGeneralOptionsView;
      var parameters = string.Empty;

      // Get the Clang Flags list
      if (!string.IsNullOrWhiteSpace(generalSettings.ClangFlags))
        parameters = GetClangFlags();

      // Get the continue when errors are detected flag 
      if (generalSettings.Continue)
        parameters += $" {ScriptConstants.kContinue}";

      // Get the verbose mode flag 
      if (generalSettings.VerboseMode)
        parameters += $" {ScriptConstants.kVerboseMode}";

      // Get the projects to ignore list 
      if (!string.IsNullOrWhiteSpace(generalSettings.ProjectsToIgnore))
        parameters += $" {ScriptConstants.kProjectsToIgnore} (''{TransformInPowerShellArray(generalSettings.ProjectsToIgnore)}'')";

      // Get the files to ignore list
      if (!string.IsNullOrWhiteSpace(generalSettings.FilesToIgnore))
        parameters += $" {ScriptConstants.kFilesToIgnore} (''{TransformInPowerShellArray(generalSettings.FilesToIgnore)}'')";

      // Get the selected Additional Includes type  
      if (0 == string.Compare(ClangGeneralAdditionalIncludesConvertor.ToString(generalSettings.AdditionalIncludes), ComboBoxConstants.kSystemIncludeDirectories))
        parameters += $" {ScriptConstants.kSystemIncludeDirectories}";

      return parameters;
    }


    /// <summary>
    /// Get the clang flags in the power shell script format
    /// </summary>
    /// <returns>The clang flags</returns>
    private string GetClangFlags()
    {
      var generalSettings = SettingsProvider.GetSettingsPage(typeof(ClangGeneralOptionsView)) as ClangGeneralOptionsView;

      return string.Format("{0} {1}", ScriptConstants.kClangFlags,
        generalSettings.TreatWarningsAsErrors ?
          $" (''{ScriptConstants.kTreatWarningsAsErrors}'',''{TransformInPowerShellArray(generalSettings.ClangFlags)}'')" :
          $" (''{TransformInPowerShellArray(generalSettings.ClangFlags)}'')");
    }


    /// <summary>
    /// Transform the UI parameters list in an power shell array 
    /// </summary>
    /// <param name="aParametersList">The list of UI parameters</param>
    /// <returns>The power shell array containing the UI parameters</returns>
    private string TransformInPowerShellArray(string aParametersList) =>
      string.Join("'',''", aParametersList.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));


    /// <summary>
    /// Get the parameters from the Tidy related option page
    /// </summary>
    /// <returns></returns>
    private string GetTidyParameters()
    {
      var tidySettings = SettingsProvider.GetSettingsPage(typeof(ClangTidyOptionsView)) as ClangTidyOptionsView;

      // Get the clang tidy parameters depending on the tidy mode
      var clangTidyParametersFactory = new ClangTidyModeParametersFactory();
      var parameters = clangTidyParametersFactory.Create(
        ClangTidyUseChecksFromConvertor.ToString(tidySettings.UseChecksFrom), ref mUseClangTidyConfigFile);

      // Append the clang tidy type(tidy / tidy-fix) with / without clang tidy config file option attached  
      if (!string.IsNullOrWhiteSpace(parameters))
        parameters = AppendClangTidyType(parameters);

      // Get the header filter option 
      if (null != tidySettings.HeaderFilter && !string.IsNullOrWhiteSpace(tidySettings.HeaderFilter.HeaderFilters))
        parameters += $" {GetHeaderFilters()}";

      return parameters;
    }


    /// <summary>
    /// Append the clang tidy type(tidy / tidy-fix) with/ without tidy config file option attached
    /// </summary>
    /// <param name="aParameters"></param>
    /// <returns>The <"aParameters"> value with the clang tidy type with / without the clang tidy config file option attached</returns>
    private string AppendClangTidyType(string aParameters)
    {
      return string.Format("{0} ''{1}{2}''",
        (mTidyFixFlag ? ScriptConstants.kTidyFix : ScriptConstants.kTidy),
        (mUseClangTidyConfigFile ? "" : "-*"),
        aParameters);
    }


    /// <summary>
    /// Get the header filter option from the Clang Tidy Option page
    /// </summary>
    /// <returns>Header filter option</returns>
    private string GetHeaderFilters()
    {
      var tidySettings = SettingsProvider.GetSettingsPage(typeof(ClangTidyOptionsView)) as ClangTidyOptionsView;

      return string.Format("{0} ''{1}''", ScriptConstants.kHeaderFilter,
        string.IsNullOrWhiteSpace(ClangTidyHeaderFiltersConvertor.ScriptEncode(tidySettings.HeaderFilter.HeaderFilters)) ?
           tidySettings.HeaderFilter.HeaderFilters :
           ClangTidyHeaderFiltersConvertor.ScriptEncode(tidySettings.HeaderFilter.HeaderFilters));
    }


    #endregion


    #endregion

  }
}
