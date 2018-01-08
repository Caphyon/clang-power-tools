using ClangPowerTools.Convertors;
using System.ComponentModel;

namespace ClangPowerTools.DialogPages
{
  public class ClangFormatPage : ConfigurationPage<ClangFormatOptions>
  {
    #region Members

    private const string kGeneralSettingsFileName = "FormatConfiguration.config";
    private SettingsPathBuilder mSettingsPathBuilder = new SettingsPathBuilder();

    #endregion

    #region Properties 

    #region Format On Save

    [Category("Format On Save")]
    [DisplayName("Enable")]
    [Description("")]
    public bool EnableFormatOnSave { get; set; }

    [Category("Format On Save")]
    [DisplayName("File Extensions")]
    [Description("")]
    public string FileExtensions { get; set; }

    #endregion

    #region Format Options

    [Category("Format Options")]
    [DisplayName("Assume File Name")]
    [Description("")]
    public string AssumeFilename { get; set; }

    [Category("Format Options")]
    [DisplayName("Fallback Style")]
    [Description("")]
    [TypeConverter(typeof(FallbackStyleConvertor))]
    public string FallbackStyle { get; set; }

    [Category("Format Options")]
    [DisplayName("Sort includes")]
    [Description("")]
    public bool SortIncludes { get; set; }

    [Category("Format Options")]
    [DisplayName("Style")]
    [Description("")]
    [TypeConverter(typeof(StyleConvertor))]
    public string Style { get; set; }

    #endregion

    public override void SaveSettingsToStorage()
    {
      string path = mSettingsPathBuilder.GetPath(kGeneralSettingsFileName);

      var updatedConfig = LoadFromFile(path);
      updatedConfig.EnableFormatOnSave = this.EnableFormatOnSave;
      updatedConfig.FileExtensions = this.FileExtensions;
      updatedConfig.AssumeFilename = this.AssumeFilename;
      updatedConfig.FallbackStyle = this.FallbackStyle;
      updatedConfig.SortIncludes = this.SortIncludes;
      updatedConfig.Style = this.Style;

      SaveToFile(path, updatedConfig);
    }

    public override void LoadSettingsFromStorage()
    {
      string path = mSettingsPathBuilder.GetPath(kGeneralSettingsFileName);
      var loadedConfig = LoadFromFile(path);

      this.EnableFormatOnSave = loadedConfig.EnableFormatOnSave;
      this.FileExtensions = loadedConfig.FileExtensions;
      this.AssumeFilename = loadedConfig.AssumeFilename;

      this.FallbackStyle = (null == loadedConfig.FallbackStyle || string.Empty == loadedConfig.FallbackStyle) ?
        ComboBoxConstants.kLLVM : loadedConfig.FallbackStyle;

      this.SortIncludes = loadedConfig.SortIncludes;

      this.Style = (null == loadedConfig.Style || string.Empty == loadedConfig.Style) ?
        ComboBoxConstants.kGoogle : loadedConfig.Style;
    }

    #endregion

  }
}
