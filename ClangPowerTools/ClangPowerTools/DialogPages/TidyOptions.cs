using Microsoft.VisualStudio.Shell;
using System.ComponentModel;

namespace ClangPowerTools
{
  public class TidyOptions : DialogPage
  {
    #region Members

    private string[] mTidyChecks;

    #endregion

    #region Methods

    public override void LoadSettingsFromStorage()
    {
      base.LoadSettingsFromStorage();
      TidyChecks = UseUserSettings ? mTidyChecks : DefaultOptions.kTidyChecks;
      UseUserSettings = true;
    }

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

  }
}
