namespace ClangPowerTools.Script
{
  public class ClangTidyModeParametersFactory
  {
    #region Methods 


    #region Public Methods

    /// <summary>
    /// Create the clang tidy parameters depending on the tidy mode
    /// </summary>
    /// <param name="aTidyMode">The searched tidy mode</param>
    /// <param name="aUseClangTidyFileFlag">Will be set to True if the clang tidy config file will be used. Will be set to False otherwise</param>
    /// <returns>Clang tidy parameters</returns>
    public string Create(string aTidyMode, ref bool aUseClangTidyFileFlag)
    {
      if (0 == string.Compare(ComboBoxConstants.kTidyFile, aTidyMode))
        return UseClangConfigFile(ref aUseClangTidyFileFlag);

      else if (0 == string.Compare(ComboBoxConstants.kCustomChecks, aTidyMode))
        return GetChecks();

      return string.Empty;
    }


    #endregion


    #region Private Methods 


    /// <summary>
    /// Get the use clang config file tag 
    /// </summary>
    /// <returns>The use clang config file tag </returns>
    private string UseClangConfigFile(ref bool aUseClangTidyFileFlag)
    {
      aUseClangTidyFileFlag = true;
      return ScriptConstants.kTidyFile;
    }


    /// <summary>
    /// Get the clang tidy parameters from the Custom Checks option page
    /// </summary>
    /// <returns></returns>
    private string GetChecks()
    {
      string tidyChecks = SettingsProvider.TidySettingsViewModel.TidyModel.PredefinedChecks;

      return !string.IsNullOrWhiteSpace(tidyChecks) ?
        $",{tidyChecks.Replace(';', ',')}" :
        string.Empty;
    }

    #endregion

    #endregion

  }
}
