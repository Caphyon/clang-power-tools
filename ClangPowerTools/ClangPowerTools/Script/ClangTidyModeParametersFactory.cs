using ClangPowerTools.DialogPages;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

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
        return GetCustomChecks();

      else if (0 == string.Compare(ComboBoxConstants.kPredefinedChecks, aTidyMode))
        return GetTidyPredefinedChecks();

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
    private string GetCustomChecks()
    {
      var tidyCustomShecksSettings = SettingsProvider.TidyCustomCheckes;

      return !string.IsNullOrWhiteSpace(tidyCustomShecksSettings.TidyChecks) ?
        $",{tidyCustomShecksSettings.TidyChecks.Replace(';', ',')}" :
        string.Empty;
    }


    /// <summary>
    /// Get the clang tidy parameters from the Predefined Checks option page
    /// </summary>
    /// <returns>The predefined checks</returns>
    private string GetTidyPredefinedChecks()
    {
      var tidyPredefinedChecksSettings = SettingsProvider.TidyPredefinedChecks;
      var parameters = string.Empty;

      foreach (PropertyInfo prop in tidyPredefinedChecksSettings.GetType().GetProperties())
      {
        object[] propAttrs = prop.GetCustomAttributes(false);
        object clangCheckAttr = propAttrs.FirstOrDefault(attr => typeof(ClangCheckAttribute) == attr.GetType());
        object displayNameAttrObj = propAttrs.FirstOrDefault(attr => typeof(DisplayNameAttribute) == attr.GetType());

        if (null == clangCheckAttr || null == displayNameAttrObj)
          continue;

        DisplayNameAttribute displayNameAttr = (DisplayNameAttribute)displayNameAttrObj;
        var value = prop.GetValue(tidyPredefinedChecksSettings, null);
        if (Boolean.TrueString != value.ToString())
          continue;
        parameters = $"{parameters},{displayNameAttr.DisplayName}";
      }

      return parameters;
    }


    #endregion

    #endregion

  }
}
