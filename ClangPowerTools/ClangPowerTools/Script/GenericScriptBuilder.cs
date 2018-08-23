using ClangPowerTools.Builder;
using ClangPowerTools.Convertors;
using ClangPowerTools.DialogPages;
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

    private ClangGeneralOptionsView mGeneralOptions;
    private ClangTidyOptionsView mTidyOptions;
    private ClangTidyPredefinedChecksOptionsView mTidyChecks;
    private ClangTidyCustomChecksOptionsView mTidyCustomChecks;
    private ClangFormatOptionsView mClangFormatView;
    private DTE2 mDTEObj;

    private string mVsEdition;
    private string mVsVersion;

    private bool mTidyFixFlag;
    private bool mUseClangTidyConfigFile;


    #endregion


    #region Constructor

    /// <summary>
    /// Instance constructor
    /// </summary>
    public GenericScriptBuilder(ClangGeneralOptionsView aGeneralOptions, ClangTidyOptionsView aTidyOptions, ClangTidyPredefinedChecksOptionsView aTidyChecks,
      ClangTidyCustomChecksOptionsView aTidyCustomChecks, ClangFormatOptionsView aClangFormatView, DTE2 aDTEObj, string aVsEdition, string aVsVersion, bool aTidyFixFlag = false)
    {
      mGeneralOptions = aGeneralOptions;
      mTidyOptions = aTidyOptions;
      mTidyChecks = aTidyChecks;
      mTidyCustomChecks = aTidyCustomChecks;
      mClangFormatView = aClangFormatView;
      mDTEObj = aDTEObj;
      mVsEdition = aVsEdition;
      mVsVersion = aVsVersion;
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
      mScript = $"{GetGeneralParameters()} {(null != mTidyOptions ? GetTidyParameters() : ScriptConstants.kParallel)}";

      // Append the clang-format style
      if (null != mClangFormatView && null != mTidyOptions && mTidyFixFlag && mTidyOptions.FormatAfterTidy)
        mScript += $" {ScriptConstants.kClangFormatStyle} {mClangFormatView.Style}";

      // Append the Visual Studio Version and Edition
      mScript += $" {ScriptConstants.kVsVersion} {mVsVersion} {ScriptConstants.kVsEdition} {mVsEdition}";

      // Append the solution path
      mScript += $" {ScriptConstants.kDirectory} ''{mDTEObj.Solution.FullName}''";
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
      var parameters = string.Empty;

      // Get the Clang Flags list
      if (!string.IsNullOrWhiteSpace(mGeneralOptions.ClangFlags))
        parameters = GetClangFlags();

      // Get the continue when errors are detected flag 
      if (mGeneralOptions.Continue)
        parameters += $" {ScriptConstants.kContinue}";

      // Get the verbose mode flag 
      if (mGeneralOptions.VerboseMode)
        parameters += $" {ScriptConstants.kVerboseMode}";

      // Get the projects to ignore list 
      if (!string.IsNullOrWhiteSpace(mGeneralOptions.ProjectsToIgnore))
        parameters += $" {ScriptConstants.kProjectsToIgnore} (''{TransformInPowerShellArray(mGeneralOptions.ProjectsToIgnore)}'')";

      // Get the files to ignore list
      if (!string.IsNullOrWhiteSpace(mGeneralOptions.FilesToIgnore))
        parameters += $" {ScriptConstants.kFilesToIgnore} (''{TransformInPowerShellArray(mGeneralOptions.FilesToIgnore)}'')";

      // Get the selected Additional Includes type  
      if (0 == string.Compare(ClangGeneralAdditionalIncludesConvertor.ToString(mGeneralOptions.AdditionalIncludes), ComboBoxConstants.kSystemIncludeDirectories))
        parameters += $" {ScriptConstants.kSystemIncludeDirectories}";

      return parameters;
    }


    /// <summary>
    /// Get the clang flags in the power shell script format
    /// </summary>
    /// <returns>The clang flags</returns>
    private string GetClangFlags()
    {
      return string.Format("{0} {1}", ScriptConstants.kClangFlags,
        mGeneralOptions.TreatWarningsAsErrors ?
          $" (''{ScriptConstants.kTreatWarningsAsErrors}'',''{TransformInPowerShellArray(mGeneralOptions.ClangFlags)}'')" :
          $" (''{TransformInPowerShellArray(mGeneralOptions.ClangFlags)}'')");
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
      // Get the clang tidy parameters depending on the tidy mode
      var clangTidyParametersFactory = new ClangTidyModeParametersFactory(mTidyCustomChecks, mTidyChecks);
      var parameters = clangTidyParametersFactory.Create(
        ClangTidyUseChecksFromConvertor.ToString(mTidyOptions.UseChecksFrom), ref mUseClangTidyConfigFile);

      // Get the clang tidy type(tidy / tidy-fix) with / without clang tidy config file option attached  
      if (string.IsNullOrWhiteSpace(parameters))
        parameters = GetClangTidyType(parameters);

      // Get the header filter option 
      if (null != mTidyOptions.HeaderFilter && !string.IsNullOrWhiteSpace(mTidyOptions.HeaderFilter.HeaderFilters))
        parameters += $" {GetHeaderFilters()}";

      return parameters;
    }


    /// <summary>
    /// Get the clang tidy type(tidy / tidy-fix) with/ without tidy config file option attached
    /// </summary>
    /// <param name="aParameters"></param>
    /// <returns>The clang tidy type with / without the clang tidy config file option attached</returns>
    private string GetClangTidyType(string aParameters)
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
      return string.Format("{0} ''{1}''", ScriptConstants.kHeaderFilter, 
        string.IsNullOrWhiteSpace(ClangTidyHeaderFiltersConvertor.ScriptEncode(mTidyOptions.HeaderFilter.HeaderFilters)) ?
           mTidyOptions.HeaderFilter.HeaderFilters :
           ClangTidyHeaderFiltersConvertor.ScriptEncode(mTidyOptions.HeaderFilter.HeaderFilters));
    }


    #endregion


    #endregion

  }
}
