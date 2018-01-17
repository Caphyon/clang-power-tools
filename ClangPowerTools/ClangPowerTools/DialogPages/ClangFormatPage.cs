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
    [DisplayName("File extensions")]
    [Description("")]
    public string FileExtensions { get; set; }

    [Category("Format On Save")]
    [DisplayName("Skip files")]
    [Description("")]
    public string SkipFiles { get; set; }

    #endregion

    #region Format Options

    [Category("Format Options")]
    [DisplayName("Assume filename")]
    [Description("")]
    public string AssumeFilename { get; set; }

    [Category("Format Options")]
    [DisplayName("Fallback style")]
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

    public ClangFormatPage Clone()
    {
      // Use MemberwiseClone to copy value types.
      var clone = (ClangFormatPage)MemberwiseClone();
      return clone;
    }

    public override void SaveSettingsToStorage()
    {
      string path = mSettingsPathBuilder.GetPath(kGeneralSettingsFileName);

      var updatedConfig = LoadFromFile(path);
      updatedConfig.EnableFormatOnSave = this.EnableFormatOnSave;
      updatedConfig.FileExtensions = this.FileExtensions;
      updatedConfig.SkipFiles = this.SkipFiles;
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

      this.FileExtensions = null == loadedConfig.FileExtensions?
        DefaultOptions.kFileExtensions : loadedConfig.FileExtensions;

      this.SkipFiles = null == loadedConfig.SkipFiles?
        DefaultOptions.kSkipFiles : loadedConfig.SkipFiles;

      this.AssumeFilename = loadedConfig.AssumeFilename;

      this.FallbackStyle = null == loadedConfig.FallbackStyle ?
        ComboBoxConstants.kNone : loadedConfig.FallbackStyle;

      this.SortIncludes = loadedConfig.SortIncludes;

      this.Style = null == loadedConfig.Style?
        ComboBoxConstants.kFile : loadedConfig.Style;

    }

    #endregion

  }
}
