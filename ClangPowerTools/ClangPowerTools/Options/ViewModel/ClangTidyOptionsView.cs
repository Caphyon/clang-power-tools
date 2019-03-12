using ClangPowerTools.Convertors;
using ClangPowerTools.Options;
using ClangPowerTools.Options.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace ClangPowerTools
{
  [Serializable]
  public class ClangTidyOptionsView : ConfigurationPage<ClangTidyOptions>, INotifyPropertyChanged
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
    private ClangTidyPathValue mClangTidyPath;
    private HeaderFiltersValue mHeaderFilters;
    private ClangTidyUseChecksFrom? mUseChecksFrom;

    public event PropertyChangedEventHandler PropertyChanged;

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
    public HeaderFiltersValue HeaderFilter
    {
      get
      {
        return mHeaderFilters;
      }
      set
      {
        mHeaderFilters = value;
        OnPropertyChanged("HeaderFilter");
      }
    }


    [Category(" Tidy")]
    [DisplayName("Use checks from")]
    [Description("Tidy checks: switch between explicitly specified checks (predefined or custom) and using checks from .clang-tidy configuration files.\n" +
      "Other options are always loaded from .clang-tidy files.")]
    [TypeConverter(typeof(ClangTidyUseChecksFromConvertor))]
    public ClangTidyUseChecksFrom? UseChecksFrom
    {
      get
      {
        return mUseChecksFrom;
      }
      set
      {
        mUseChecksFrom = value;
        OnPropertyChanged("UseChecksFrom");
      }
    }


    [Category("Clang-Tidy")]
    [DisplayName("Use custom executable file")]
    [Description("Specify a custom path for \"clang-tidy.exe\" file to run instead of the built-in one (v6.0)")]
    [ClangTidyPathAttribute(true)]
    public ClangTidyPathValue ClangTidyPath
    {
      get
      {
        return mClangTidyPath;
      }
      set
      {
        mClangTidyPath = value;
        OnPropertyChanged("ClangTidyPath");
      }
    }


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

      ClangTidyOptions updatedConfig = LoadFromFile(path);

      updatedConfig.AutoTidyOnSave = AutoTidyOnSave;
      updatedConfig.FormatAfterTidy = FormatAfterTidy;

      updatedConfig.HeaderFilter =
        true == string.IsNullOrWhiteSpace(ClangTidyHeaderFiltersConvertor.ScriptEncode(HeaderFilter.HeaderFilters)) ?
          HeaderFilter.HeaderFilters : ClangTidyHeaderFiltersConvertor.ScriptEncode(HeaderFilter.HeaderFilters);

      updatedConfig.TidyMode = UseChecksFrom;
      updatedConfig.ClangTidyPath = ClangTidyPath;

      SaveToFile(path, updatedConfig);

      SetEnvironmentVariableTidyPath();
    }


    public override void LoadSettingsFromStorage()
    {
      string path = mSettingsPathBuilder.GetPath(kTidyOptionsFileName);
      ClangTidyOptions loadedConfig = LoadFromFile(path);

      AutoTidyOnSave = loadedConfig.AutoTidyOnSave;
      FormatAfterTidy = loadedConfig.FormatAfterTidy;

      if (loadedConfig.HeaderFilter == null)
      {
        HeaderFilter = new HeaderFiltersValue(ComboBoxConstants.kDefaultHeaderFilter);
      }
      else if (string.IsNullOrWhiteSpace(ClangTidyHeaderFiltersConvertor.ScriptDecode(loadedConfig.HeaderFilter)) == false)
      {
        HeaderFilter = new HeaderFiltersValue(ClangTidyHeaderFiltersConvertor.ScriptDecode(loadedConfig.HeaderFilter));
      }
      else
      {
        HeaderFilter = new HeaderFiltersValue(loadedConfig.HeaderFilter);
      }

      if (loadedConfig.TidyMode == null)
      {
        UseChecksFrom = string.IsNullOrWhiteSpace(loadedConfig.TidyChecksCollection) ?
          ClangTidyUseChecksFrom.PredefinedChecks : ClangTidyUseChecksFrom.CustomChecks;
      }
      else
      {
        UseChecksFrom = loadedConfig.TidyMode;
      }

      if (loadedConfig.ClangTidyPath == null)
      {
        ClangTidyPath = new ClangTidyPathValue();
      }
      else
      {
        ClangTidyPath = loadedConfig.ClangTidyPath;
      }

      SetEnvironmentVariableTidyPath();
    }


    #endregion


    #region Private Methods

    private void OnPropertyChanged(string aPropName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(aPropName));
    }


    private void SetEnvironmentVariableTidyPath()
    {
      var task = Task.Run(() =>
      {
        if (null != ClangTidyPath && ClangTidyPath.Enable == true && ClangTidyPath.Value.Length > 0)
        {
          Environment.SetEnvironmentVariable(ScriptConstants.kEnvrionmentTidyPath, ClangTidyPath.Value, EnvironmentVariableTarget.User);
        }
        else
        {
          Environment.SetEnvironmentVariable(ScriptConstants.kEnvrionmentTidyPath, null, EnvironmentVariableTarget.User);
        }
      });
    }

    #endregion

  }
}
