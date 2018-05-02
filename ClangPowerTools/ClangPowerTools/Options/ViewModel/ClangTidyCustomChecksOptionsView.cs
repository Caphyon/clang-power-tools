﻿using ClangPowerTools.Options.View;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace ClangPowerTools.DialogPages
{
  public class ClangTidyCustomChecksOptionsView : ConfigurationPage<ClangTidyOptions>
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
        TidyChecksCollection = this.TidyChecks
      };

      SaveToFile(path, updatedConfig);
    }

    public override void LoadSettingsFromStorage()
    {
      string path = mSettingsPathBuilder.GetPath(kTidyOptionsFileName);
      var loadedConfig = LoadFromFile(path);

      if (null == loadedConfig.TidyChecks || 0 == loadedConfig.TidyChecks.Count)
        this.TidyChecks = loadedConfig.TidyChecksCollection;
      else
        this.TidyChecks = string.Join(";", loadedConfig.TidyChecks);

    }

    #endregion


  }
}
