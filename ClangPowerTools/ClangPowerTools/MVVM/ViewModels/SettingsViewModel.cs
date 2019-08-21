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
        cptSettings.LoadSettings();
      }
      else if(cptSettings.OldGeneralSettingsExists())
      {
        cptSettings.MapOldSettings();
        cptSettings.SaveSettings();
        cptSettings.DeleteOldSettings();
      }
    }
    #endregion


    #region Methods
    public void OnClosed(object sender, EventArgs e)
    {
      cptSettings.SaveSettings();
      settingsView.Closed -= OnClosed;
    }


    #endregion
  }
}
