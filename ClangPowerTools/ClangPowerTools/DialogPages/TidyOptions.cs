using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI.WebControls;

namespace ClangPowerTools
{
  [Serializable]
  public class TidyOptions : DialogPage
  {
    #region Members

    private string[] mTidyChecks;
    private const string kTidyOptionsFileName = "TidyOptionsConfiguration.config";
    private SettingsPathBuilder mSettingsPathBuilder = new SettingsPathBuilder();

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

    #endregion

    #region DialogPage Save and Load implementation 

    public override void SaveSettingsToStorage()
    {
      string path = mSettingsPathBuilder.GetPath(kTidyOptionsFileName);

      var currentSettings = new ClangTidyOptions
      {
        Fix = this.Fix,
        TidyChecks = this.TidyChecks.ToList()
      };

      XmlSerializer serializer = new XmlSerializer();
      serializer.SerializeToFile(path, currentSettings);
    }

    public override void LoadSettingsFromStorage()
    {
      string path = mSettingsPathBuilder.GetPath(kTidyOptionsFileName);
      XmlSerializer serializer = new XmlSerializer();

      var loadedConfig = File.Exists(path)
        ? serializer.DeserializeFromFIle<ClangTidyOptions>(path)
        : new ClangTidyOptions();

      this.TidyChecks = loadedConfig.TidyChecks.ToArray();
      this.Fix = loadedConfig.Fix;
    }

    #endregion


  }
}
