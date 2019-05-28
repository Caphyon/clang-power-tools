using ClangPowerTools.Options.View;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace ClangPowerTools
{
  public class ClangGeneralOptionsView : ConfigurationPage<ClangOptions>, INotifyPropertyChanged
  {
    #region Members

    private string mClangFlags = string.Empty;
    private const string kGeneralSettingsFileName = "GeneralConfiguration.config";
    private SettingsPathBuilder mSettingsPathBuilder = new SettingsPathBuilder();
    private string mFilesToIgnore = string.Empty;

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion

    #region Properties


    [Category("General")]
    [DisplayName("Compile flags")]
    [Description("Flags given to clang++ when compiling project, alongside project - specific defines. If empty the default flags will be loaded.")]
    public string ClangFlags
    {
      get => string.IsNullOrWhiteSpace(mClangFlags) ? DefaultOptions.kClangFlags : mClangFlags;
      set => mClangFlags = value;
    }


    [Category("General")]
    [DisplayName("File to ignore")]
    [Description("Array of file(s) to ignore, from the matched ones. If empty, all already matched files are compiled.")]
    public string FilesToIgnore
    {
      get { return mFilesToIgnore; }
      set
      {
        mFilesToIgnore = value;
        OnPropertyCHanged("FilesToIgnore");
      }
    }

    private void OnPropertyCHanged(string aPropName)
    {
      if (PropertyChanged != null)
      {
        PropertyChanged(this, new PropertyChangedEventArgs(aPropName));
      }
    }


    [Category("General")]
    [DisplayName("Project to ignore")]
    [Description("Array of project(s) to ignore, from the matched ones. If empty, all already matched projects are compiled.")]
    public string ProjectsToIgnore { get; set; }



    [Category("General")]
    [DisplayName("Treat additional includes as")]
    [Description("Specify how clang interprets project additional include directories: as regular includes ( -I ) or system includes ( -isystem ).")]
    public ClangGeneralAdditionalIncludes? AdditionalIncludes { get; set; }


    [Category("General")]
    [DisplayName("Treat warnings as errors")]
    [Description("Treats all compiler warnings as errors. For a new project, it may be best to use in all compilations; resolving all warnings will ensure the fewest possible hard to find code defects.")]
    public bool TreatWarningsAsErrors { get; set; }


    [Category("General")]
    [DisplayName("Continue on error")]
    [Description("Switch to continue project compilation even when errors occur.")]
    public bool Continue { get; set; }


    [Category("General")]
    [DisplayName("Clang compile after MSVC compile")]
    [Description("Automatically run Clang compile on the current source file, after successful MSVC compilation.")]
    public bool ClangCompileAfterVsCompile { get; set; }


    [Category("General")]
    [DisplayName("Verbose mode")]
    [Description("Enables verbose logging for diagnostic purposes.")]
    public bool VerboseMode { get; set; }


    [Browsable(false)]
    public string Version { get; set; }


    protected override IWin32Window Window
    {
      get
      {
        ElementHost elementHost = new ElementHost();
        elementHost.Child = new ClangGeneralOptionsUserControl(this);
        return elementHost;
      }
    }

    #endregion

    #region DialogPage Save and Load implementation 

    public override void SaveSettingsToStorage()
    {
      string path = mSettingsPathBuilder.GetPath(kGeneralSettingsFileName);

      var updatedConfig = new ClangOptions
      {
        ClangFlagsCollection = string.IsNullOrEmpty(this.ClangFlags) ?
          this.ClangFlags : this.ClangFlags.Replace(" ", "").Trim(';'),

        FilesToIgnoreCollection = string.IsNullOrEmpty(this.FilesToIgnore) ?
          this.FilesToIgnore : this.FilesToIgnore.Replace(" ", "").Trim(';'),

        ProjectsToIgnoreCollection = string.IsNullOrEmpty(this.ProjectsToIgnore) ?
          this.ProjectsToIgnore : this.ProjectsToIgnore.Replace(" ", "").Trim(';'),

        AdditionalIncludes = this.AdditionalIncludes,
        TreatWarningsAsErrors = this.TreatWarningsAsErrors,
        Continue = this.Continue,
        ClangCompileAfterVsCompile = this.ClangCompileAfterVsCompile,
        VerboseMode = this.VerboseMode,
        Version = this.Version
      };

      SaveToFile(path, updatedConfig);
    }

    public override void LoadSettingsFromStorage()
    {
      string path = mSettingsPathBuilder.GetPath(kGeneralSettingsFileName);

      XmlSerializer serializer = new XmlSerializer();
      var loadedConfig = LoadFromFile(path);

      if (null == loadedConfig.ClangFlags || 0 == loadedConfig.ClangFlags.Count)
        ClangFlags = loadedConfig.ClangFlagsCollection;
      else
        ClangFlags = string.Join(";", loadedConfig.ClangFlags);


      if (null == loadedConfig.FilesToIgnore || 0 == loadedConfig.FilesToIgnore.Count)
      {
        if (null == loadedConfig.FilesToIgnoreCollection)
        {
          FilesToIgnore = string.Empty;
        }
        else
        {
          FilesToIgnore = loadedConfig.FilesToIgnoreCollection;
        }
      }
      else
      { 
        FilesToIgnore = string.Join(";", loadedConfig.FilesToIgnore);
      }

      if (null == loadedConfig.ProjectsToIgnore || 0 == loadedConfig.ProjectsToIgnore.Count)
        ProjectsToIgnore = loadedConfig.ProjectsToIgnoreCollection ?? string.Empty;
      else
        ProjectsToIgnore = string.Join(";", loadedConfig.ProjectsToIgnore);

      AdditionalIncludes = null == loadedConfig.AdditionalIncludes ? 
        ClangGeneralAdditionalIncludes.IncludeDirectories : loadedConfig.AdditionalIncludes;

      TreatWarningsAsErrors = loadedConfig.TreatWarningsAsErrors;
      Continue = loadedConfig.Continue;
      ClangCompileAfterVsCompile = loadedConfig.ClangCompileAfterVsCompile;
      VerboseMode = loadedConfig.VerboseMode;
      Version = loadedConfig.Version;
    }

    public override void ResetSettings()
    {
      SettingsHandler.CopySettingsProperties(SettingsProvider.GeneralSettings, new ClangGeneralOptionsView());
      SaveSettingsToStorage();
      LoadSettingsFromStorage();
    }

    #endregion
  }
}
