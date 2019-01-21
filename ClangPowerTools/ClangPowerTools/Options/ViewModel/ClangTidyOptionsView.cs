using ClangPowerTools.Convertors;
using ClangPowerTools.Options;
using ClangPowerTools.Options.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace ClangPowerTools
{
  [Serializable]
  public class ClangTidyOptionsView : ConfigurationPage<ClangTidyOptions>
  {

    #region Constants

    public static List<HeaderFiltersValue> DefaultHeaderFilters
    {
      get
      {
        return new List<HeaderFiltersValue>()
        {
          new HeaderFiltersValue(ComboBoxConstants.kDefaultHeaderFilter),
          new HeaderFiltersValue(ComboBoxConstants.kCorrespondingHeaderName)
        };
      }
    }

    #endregion

    #region Members

    private const string kTidyOptionsFileName = "TidyOptionsConfiguration.config";
    private SettingsPathBuilder mSettingsPathBuilder = new SettingsPathBuilder();

    #endregion

    #region Properties

    [Category(" Tidy")]
    [DisplayName("Format after tidy")]
    [Description("")]
    public bool FormatAfterTidy { get; set; }


    [Category(" Tidy")]
    [DisplayName("Perform clang-tidy on save")]
    [Description("Automatically run clang-tidy when saving the current source file.")]
    public bool AutoTidyOnSave { get; set; }


    [Category(" Tidy")]
    [DisplayName("Header filter")]
    [Description("Regular expression matching the names of the headers to output diagnostics from or auto-fix. Diagnostics from the source file are always displayed." +
      "This option overrides the 'HeaderFilter' option in .clang-tidy file, if any.\n" +
      "\"Corresponding Header\" : output diagnostics/fix only the corresponding header (same filename) for each source file analyzed.")]
    public HeaderFiltersValue HeaderFilter { get; set; }


    [Category(" Tidy")]
    [DisplayName("Use checks from")]
    [Description("Tidy checks: switch between explicitly specified checks (predefined or custom) and using checks from .clang-tidy configuration files.\n" +
      "Other options are always loaded from .clang-tidy files.")]
    [TypeConverter(typeof(ClangTidyUseChecksFromConvertor))]
    public ClangTidyUseChecksFrom? UseChecksFrom { get; set; }


    [Category("Clang-Tidy")]
    [DisplayName("Use custom executable file")]
    [Description("Specify a custom path for \"clang-tidy.exe\" file to run instead of the built-in one (v6.0)")]
    [ClangFormatPathAttribute(true)]
    public ClangTidyPathValue ClangTidytPath { get; set; }


    protected override IWin32Window Window
    {
      get
      {
        ElementHost elementHost = new ElementHost();
        elementHost.Child = new ClangTidyOptionsUserControl(this);
        return elementHost;
      }
    }

    #endregion

    #region DialogPage Save and Load implementation 

    public override void SaveSettingsToStorage()
    {
      string path = mSettingsPathBuilder.GetPath(kTidyOptionsFileName);

      var updatedConfig = LoadFromFile(path);

      updatedConfig.AutoTidyOnSave = this.AutoTidyOnSave;
      updatedConfig.FormatAfterTidy = this.FormatAfterTidy;

      updatedConfig.HeaderFilter =
        true == string.IsNullOrWhiteSpace(ClangTidyHeaderFiltersConvertor.ScriptEncode(this.HeaderFilter.HeaderFilters)) ?
          this.HeaderFilter.HeaderFilters : ClangTidyHeaderFiltersConvertor.ScriptEncode(this.HeaderFilter.HeaderFilters);

      updatedConfig.TidyMode = this.UseChecksFrom;
      ClangTidytPath = this.ClangTidytPath;

      SaveToFile(path, updatedConfig);
    }

    public override void LoadSettingsFromStorage()
    {
      string path = mSettingsPathBuilder.GetPath(kTidyOptionsFileName);
      var loadedConfig = LoadFromFile(path);

      AutoTidyOnSave = loadedConfig.AutoTidyOnSave;
      FormatAfterTidy = loadedConfig.FormatAfterTidy;

      if (loadedConfig.HeaderFilter == null)
        HeaderFilter = new HeaderFiltersValue(ComboBoxConstants.kDefaultHeaderFilter);
      else if (string.IsNullOrWhiteSpace(ClangTidyHeaderFiltersConvertor.ScriptDecode(loadedConfig.HeaderFilter)) == false)
        HeaderFilter = new HeaderFiltersValue(ClangTidyHeaderFiltersConvertor.ScriptDecode(loadedConfig.HeaderFilter));
      else
        HeaderFilter = new HeaderFiltersValue(loadedConfig.HeaderFilter);

      if (loadedConfig.TidyMode == null)
      {
        UseChecksFrom = string.IsNullOrWhiteSpace(loadedConfig.TidyChecksCollection) ?
          ClangTidyUseChecksFrom.PredefinedChecks : ClangTidyUseChecksFrom.CustomChecks;
      }
      else
        UseChecksFrom = loadedConfig.TidyMode;

      if (loadedConfig.ClangTidyPath == null)
        ClangTidytPath = new ClangTidyPathValue();
      else
        ClangTidytPath = loadedConfig.ClangTidyPath;

    }

    #endregion

  }
}
