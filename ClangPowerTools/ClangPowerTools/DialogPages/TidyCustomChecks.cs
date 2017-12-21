using System.ComponentModel;
using System.Linq;
using System.Web.UI.WebControls;

namespace ClangPowerTools.DialogPages
{
  public class TidyCustomChecks : ConfigurationPage<ClangTidyOptions>
  {
    #region Members

    private string[] mTidyChecks;
    private const string kTidyOptionsFileName = "TidyOptionsConfiguration.config";
    private SettingsPathBuilder mSettingsPathBuilder = new SettingsPathBuilder();

    #endregion

    #region Properties

    [Category(" Tidy")]
    [DisplayName("Custom checks")]
    [Description("Specify clang-tidy checks to run using the standard tidy syntax. You can use wildcards to match multiple checks, combine them, etc (Eg. \"modernize-*, readability-*\").")]
    [TypeConverter(typeof(StringArrayConverter))]
    public string[] TidyChecks
    {
      get => mTidyChecks;
      set => mTidyChecks = value;
    }

    #endregion

    #region DialogPage Save and Load implementation 

    public override void SaveSettingsToStorage()
    {
      string path = mSettingsPathBuilder.GetPath(kTidyOptionsFileName);

      var updatedConfig = LoadFromFile(path);
      updatedConfig.TidyChecks = this.TidyChecks.ToList();

      SaveToFile(path, updatedConfig);
    }

    public override void LoadSettingsFromStorage()
    {
      string path = mSettingsPathBuilder.GetPath(kTidyOptionsFileName);
      var loadedConfig = LoadFromFile(path);
      this.TidyChecks = loadedConfig.TidyChecks.ToArray();
    }

    #endregion


  }
}
