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
    [DisplayName("Perform clang-tidy on save")]
    [Description("Automatically run clang-tidy when saving the current source file.")]
    public bool AutoTidyOnSave { get; set; }

    [Category(" Tidy")]
    [DisplayName("Fix")]
    [Description("Automatically applies clang-tidy fixes to selected source files, affected header files and saves them to disk.")]
    public bool Fix { get; set; }

    [Category(" Tidy")]
    [DisplayName("Header filter")]
    [Description("Regular expression matching the names of the headers to output diagnostics from or auto-fix. Diagnostics from the source file are always displayed." + 
      "This option overrides the 'HeaderFilter' option in .clang-tidy file, if any.\n" +
      "\"Corresponding Header\" : output diagnostics/fix only the corresponding header (same filename) for each source file analyzed.")]
    [TypeConverter(typeof(HeaderFiltersConvertor))]
    public string HeaderFilter { get; set; }

    [Category(" Tidy")]
    [DisplayName("Use checks from")]
    [Description("Tidy checks: switch between explicitly specified checks (predefined or custom) and using checks from .clang-tidy configuration files.\n" +
      "Other options are always loaded from .clang-tidy files.")]
    [TypeConverter(typeof(UseChecksFromConvertor))]
    public string UseChecksFrom { get; set; }

    [Category(" Tidy")]
    [DisplayName("Format after tidy")]
    [Description("")]
    public bool FormatAfterTidy { get; set; }

    #endregion

    #region DialogPage Save and Load implementation 

    public override void SaveSettingsToStorage()
    {
      string path = mSettingsPathBuilder.GetPath(kTidyOptionsFileName);

      var updatedConfig = LoadFromFile(path);

      updatedConfig.AutoTidyOnSave = this.AutoTidyOnSave;
      updatedConfig.Fix = this.Fix;
      updatedConfig.FormatAfterTidy = this.FormatAfterTidy;

      updatedConfig.HeaderFilter = ComboBoxConstants.kHeaderFilterMaping.ContainsKey(this.HeaderFilter) ?
        ComboBoxConstants.kHeaderFilterMaping[this.HeaderFilter] : this.HeaderFilter;

      updatedConfig.TidyMode = this.UseChecksFrom;

      SaveToFile(path, updatedConfig);
    }

    public override void LoadSettingsFromStorage()
    {
      string path = mSettingsPathBuilder.GetPath(kTidyOptionsFileName);
      var loadedConfig = LoadFromFile(path);

      this.Fix = loadedConfig.Fix;
      this.AutoTidyOnSave = loadedConfig.AutoTidyOnSave;
      this.FormatAfterTidy = loadedConfig.FormatAfterTidy;

      if (null == loadedConfig.HeaderFilter)
        this.HeaderFilter = DefaultOptions.kHeaderFilter;
      else if (ComboBoxConstants.kHeaderFilterMaping.ContainsKey(loadedConfig.HeaderFilter))
        this.HeaderFilter = ComboBoxConstants.kHeaderFilterMaping[loadedConfig.HeaderFilter];
      else
        this.HeaderFilter = loadedConfig.HeaderFilter;

      if (null == loadedConfig.TidyMode || string.Empty == loadedConfig.TidyMode)
        this.UseChecksFrom = (0 == loadedConfig.TidyChecks.Count ? ComboBoxConstants.kPredefinedChecks : ComboBoxConstants.kCustomChecks);
      else
        this.UseChecksFrom = loadedConfig.TidyMode;
    }

    #endregion

  }
}
