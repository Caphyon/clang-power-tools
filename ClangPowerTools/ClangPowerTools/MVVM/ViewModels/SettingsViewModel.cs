using ClangPowerTools.Views;
using System;

namespace ClangPowerTools
{
  public class SettingsViewModel
  {
    #region Members
    private SettingsHandler cptSettings = new SettingsHandler();
    private SettingsView settingsView;

    #endregion;

    #region Constructors
    public SettingsViewModel(SettingsView settingsView)
    {
      this.settingsView = settingsView;
      InitializeSettings();
      settingsView.Closed += OnClosed;
    }

    private void InitializeSettings()
    {
      if (cptSettings.SettingsFileExists())
      {
        cptSettings.LoadSettings(cptSettings.SettingsPath);
      }
      else if(cptSettings.OldGeneralSettingsExists())
      {
        cptSettings.MapOldSettings();
        cptSettings.SaveSettings(cptSettings.SettingsPath);
        cptSettings.DeleteOldSettings();
      }
    }
    #endregion


    #region Methods
    public void OnClosed(object sender, EventArgs e)
    {
      cptSettings.SaveSettings(cptSettings.SettingsPath);
      settingsView.Closed -= OnClosed;
    }


    #endregion
  }
}
