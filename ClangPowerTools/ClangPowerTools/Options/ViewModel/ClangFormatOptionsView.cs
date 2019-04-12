using ClangPowerTools.Options;
using ClangPowerTools.Options.ViewModel;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace ClangPowerTools.DialogPages
{
  public class ClangFormatOptionsView : ConfigurationPage<ClangFormatOptions>, INotifyPropertyChanged
  {
    #region Members

    private const string kGeneralSettingsFileName     = "FormatConfiguration.config";
    private SettingsPathBuilder mSettingsPathBuilder  = new SettingsPathBuilder();
    private string mSkipFiles;
    private ClangFormatPathValue clangFormatPath;

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion


    #region Properties 


    #region Format On Save

    [Category("Format On Save")]
    [DisplayName("Enable")]
    [Description("Enable running clang-format when modified files are saved. " +
      "Will only format if Style is found (ignores Fallback Style).")]
    public bool EnableFormatOnSave { get; set; }

    [Category("Format On Save")]
    [DisplayName("File extensions")]
    [Description("When formatting on save, clang-format will be applied only to files with these extensions.")]
    public string FileExtensions { get; set; }

    [Category("Format On Save")]
    [DisplayName("File to ignore")]
    [Description("When formatting on save, clang-format will not be applied on these files.")]
    public string FilesToIgnore
    {
      get { return mSkipFiles; }
      set
      {
        mSkipFiles = value;
        OnPropertyChanged("FilesToIgnore");
      }
    }

    private void OnPropertyChanged(string aPropName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(aPropName));
    }

    #endregion


    #region Format Options

    [Category("Format Options")]
    [DisplayName("Assume filename")]
    [Description("When reading from stdin, clang-format assumes this filename to look for a style config file" +
      "(with -style=file) and to determine the language.")]
    public string AssumeFilename { get; set; } = string.Empty;

    [Category("Format Options")]
    [DisplayName("Fallback style")]
    [Description("The name of the predefined style used as a fallback in case clang-format is invoked with " +
      "-style=file, but can not find the .clang-format file to use.\nUse -fallback-style=none to skip formatting.")]
    public ClangFormatFallbackStyle? FallbackStyle { get; set; }

    //[Category("Format Options")]
    //[DisplayName("Sort includes")]
    //[Description("If set, overrides the include sorting behavior determined by the SortIncludes style flag.")]
    //public bool SortIncludes { get; set; }

    [Category("Format Options")]
    [DisplayName("Style")]
    [Description("Coding style, currently supports: LLVM, Google, Chromium, Mozilla, WebKit.\nUse -style=file to load " +
      "style configuration from .clang-format file located in one of the parent directories of the " +
      "source file(or current directory for stdin).\nUse -style=\"{key: value, ...}\" to set specific parameters, " +
      "e.g.: -style=\"{BasedOnStyle: llvm, IndentWidth: 8}\"")]
    public ClangFormatStyle? Style { get; set; }


    [Category("Clang-Format")]
    [DisplayName("Use custom executable file")]
    [Description("Specify a custom path for \"clang-format.exe\" file to run instead of the built-in one (v6.0)")]
    [ClangFormatPathAttribute(true)]
    public ClangFormatPathValue ClangFormatPath
    {
      get
      {
        return clangFormatPath;
      }
      set
      {
        clangFormatPath = value;
        OnPropertyChanged("ClangFormatPath");
      }
    }

    #endregion


    protected override IWin32Window Window
    {
      get
      {
        ElementHost elementHost   = new ElementHost();
        elementHost.Child         = new ClangFormatOptionsUserControl(this);
        return elementHost;
      }
    }

    #endregion


    #region DialogPage Save and Load implementation 


    public override void SaveSettingsToStorage()
    {
      string path = mSettingsPathBuilder.GetPath(kGeneralSettingsFileName);

      var updatedConfig = new ClangFormatOptions
      {
        EnableFormatOnSave = this.EnableFormatOnSave,

        FileExtensions = string.IsNullOrEmpty(this.FileExtensions) ?
          this.FileExtensions : this.FileExtensions.Replace(" ", "").Trim(';'),

        SkipFiles = string.IsNullOrEmpty(this.FilesToIgnore) ?
          this.FilesToIgnore : this.FilesToIgnore.Replace(" ", "").Trim(';'),

        AssumeFilename = string.IsNullOrEmpty(this.AssumeFilename) ?
          this.AssumeFilename : this.AssumeFilename.Replace(" ", "").Trim(';'),

        FallbackStyle     = this.FallbackStyle,
        //SortIncludes    = this.SortIncludes,
        Style             = this.Style,
        ClangFormatPath   = this.ClangFormatPath

      };

      SaveToFile(path, updatedConfig);
    }

    public override void LoadSettingsFromStorage()
    {
      string path             = mSettingsPathBuilder.GetPath(kGeneralSettingsFileName);
      var loadedConfig        = LoadFromFile(path);
      EnableFormatOnSave = loadedConfig.EnableFormatOnSave;

      FileExtensions = null == loadedConfig.FileExtensions ?
        DefaultOptions.kFileExtensions : loadedConfig.FileExtensions;

      FilesToIgnore = null == loadedConfig.SkipFiles ?
        DefaultOptions.kSkipFiles : loadedConfig.SkipFiles;

      AssumeFilename = loadedConfig.AssumeFilename;

      FallbackStyle = null == loadedConfig.FallbackStyle ?
        ClangFormatFallbackStyle.none : loadedConfig.FallbackStyle;

      Style = null == loadedConfig.Style ? ClangFormatStyle.file : loadedConfig.Style;

      //SortIncludes   = loadedConfig.SortIncludes;

      if (null == loadedConfig.ClangFormatPath)
        ClangFormatPath = new ClangFormatPathValue();
      else
        ClangFormatPath = loadedConfig.ClangFormatPath;

    }

    #endregion


    #region Public Methods

    public ClangFormatOptionsView Clone()
    {
      // Use MemberwiseClone to copy value types.
      var clone = (ClangFormatOptionsView)MemberwiseClone();
      return clone;
    }

    #endregion

  }
}
