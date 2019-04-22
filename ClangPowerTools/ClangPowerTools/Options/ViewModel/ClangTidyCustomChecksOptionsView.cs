using ClangPowerTools.Options.View;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace ClangPowerTools
{
  public class ClangTidyCustomChecksOptionsView : ConfigurationPage<ClangTidyOptions>
  {
    #region Members

    private const string kTidyOptionsFileName = "TidyOptionsConfiguration.config";
    private SettingsPathBuilder mSettingsPathBuilder = new SettingsPathBuilder();

    #endregion

    #region Properties

    [Category(" Tidy")]
    [DisplayName("Custom checks")]
    [Description("Specify clang-tidy checks to run using the standard tidy syntax. You can use wildcards to match multiple checks, combine them, etc (Eg. \"modernize-*, readability-*\").")]
    public string TidyChecks { get; set; }

    protected override IWin32Window Window
    {
      get
      {
        ElementHost elementHost = new ElementHost();
        elementHost.Child = new ClangTidyCustomChecksUserControl(this);
        return elementHost;
      }
    }

    #endregion

    #region DialogPage Save and Load implementation 

    public override void SaveSettingsToStorage()
    {
      string path = mSettingsPathBuilder.GetPath(kTidyOptionsFileName);
      var updatedConfig = new ClangTidyOptions
      {
        TidyChecksCollection = string.IsNullOrEmpty(this.TidyChecks) ?
          this.TidyChecks : this.TidyChecks.Replace(" ", "").Trim(';')
      };

      SaveToFile(path, updatedConfig);
    }

    public override void LoadSettingsFromStorage()
    {
      string path = mSettingsPathBuilder.GetPath(kTidyOptionsFileName);
      var loadedConfig = LoadFromFile(path);

      if (null == loadedConfig.TidyChecks || 0 == loadedConfig.TidyChecks.Count)
        TidyChecks = loadedConfig.TidyChecksCollection ?? string.Empty;
      else
        TidyChecks = string.Join(";", loadedConfig.TidyChecks);

    }

    public override void ResetSettings()
    {
      SettingsHandler.CopySettingsProperties(new ClangTidyCustomChecksOptionsView(), SettingsProvider.TidyCustomCheckes);
      SaveSettingsToStorage();
      LoadSettingsFromStorage();
    }

    #endregion

  }
}
