using ClangPowerTools.Properties;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel;
using System.Linq;
using System.Web.UI.WebControls;

namespace ClangPowerTools
{
  [Serializable]
  public class TidyOptions : DialogPage
  {
    #region Members

    private string[] mTidyChecks;

    #endregion

    #region Properties

    [Category(" Tidy")]
    [DisplayName("Custom Checks")]
    [Description("If not empty clang-tidy will be called with given flags, instead of clang++. The tidy operation is applied to whole translation units, meaning all directory headers included in the CPP will be tidied up too. Changes will not be applied, only simulated.")]
    [TypeConverter(typeof(StringArrayConverter))]
    public string[] TidyChecks
    {
      get => mTidyChecks;
      set => mTidyChecks = value;
    }

    [Category(" Tidy")]
    [DisplayName("Fix")]
    [Description("If not empty clang-tidy will be called with given flags, instead of clang++. The tidy operation is applied to whole translation units, meaning all directory headers included in the CPP will be tidied up too. Changes will be applied to the file(s).")]
    public bool Fix { get; set; }

    [Category(" Tidy")]
    [DisplayName("Use User Settings")]
    [Description("If not empty clang-tidy will be called with given flags, instead of clang++. The tidy operation is applied to whole translation units, meaning all directory headers included in the CPP will be tidied up too. Changes will not be applied, only simulated.")]
    [Browsable(false)]
    public bool UseUserSettings { get; set; }

    #endregion

    [Browsable(false)]
    public ClangTidyOptions SavedUserSettings
    {
      get { return Settings.Default.TidyOptions; }
      set { Settings.Default.TidyOptions = value; }
    }

    public override void SaveSettingsToStorage()
    {
      SavedUserSettings.TidyChecks = this.TidyChecks.ToList();
      SavedUserSettings.Fix = this.Fix;
      SavedUserSettings.UseUserSettings = this.UseUserSettings;

      base.SaveSettingsToStorage();
      Settings.Default.Save();
    }

    #region Methods

    public override void LoadSettingsFromStorage()
    {
      if (SavedUserSettings == null)
        SavedUserSettings = new ClangTidyOptions();

      this.UseUserSettings = SavedUserSettings.UseUserSettings;
      this.TidyChecks = UseUserSettings ? SavedUserSettings.TidyChecks.ToArray() : DefaultOptions.kTidyChecks;
      this.Fix = SavedUserSettings.Fix;

      base.LoadSettingsFromStorage();
    }

    #endregion


  }
}
