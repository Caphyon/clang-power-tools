using System;
using System.ComponentModel;

namespace ClangPowerTools
{
  [Serializable]
  public class TidyOptions : ConfigurationPage<ClangTidyOptions>
  {
    #region Members

    private const string kTidyOptionsFileName = "TidyOptionsConfiguration.config";
    private SettingsPathBuilder mSettingsPathBuilder = new SettingsPathBuilder();

    #endregion

    #region Properties

    [Category(" Tidy")]
    [DisplayName("Fix")]
    [Description("Automatically applies clang-tidy fixes to selected source files, affected header files and saves them to disk.")]
    public bool Fix { get; set; }

    [Category(" Tidy")]
    [DisplayName("Header filter")]
    [Description("")]
    public string HeaderFilter { get; set; }

    [Category(" Tidy")]
    [DisplayName("Use checks from")]
    [Description("Tidy checks: switch between explicitly specified checks (predefined or custom) and using checks from .clang-tidy configuration files.\nOther options are always loaded from .clang-tidy files.")]
    [TypeConverter(typeof(TidyModeConvertor))]
    public string TidyMode { get; set; }

    #endregion

    #region DialogPage Save and Load implementation 

    public override void SaveSettingsToStorage()
    {
      string path = mSettingsPathBuilder.GetPath(kTidyOptionsFileName);

      var updatedConfig = LoadFromFile(path);
      updatedConfig.Fix = this.Fix;
      updatedConfig.HeaderFilter = this.HeaderFilter;
      updatedConfig.TidyMode = this.TidyMode;

      SaveToFile(path, updatedConfig);
    }

    public override void LoadSettingsFromStorage()
    {
      string path = mSettingsPathBuilder.GetPath(kTidyOptionsFileName);
      var loadedConfig = LoadFromFile(path);

      this.Fix = loadedConfig.Fix;
      this.HeaderFilter = null == loadedConfig.HeaderFilter ? 
        DefaultOptions.kHeaderFilter : loadedConfig.HeaderFilter;

      if (null == loadedConfig.TidyMode || string.Empty == loadedConfig.TidyMode)
        this.TidyMode = (0 == loadedConfig.TidyChecks.Count ? TidyModeConstants.kPredefinedChecks : TidyModeConstants.kCustomChecks);
      else
        this.TidyMode = loadedConfig.TidyMode;
    }

    #endregion

  }
}
