using ClangPowerTools.Views;
using System;

namespace ClangPowerTools
{
  public class SettingsViewModel
  {
    #region Members
    private SettingsHandler settingsHandler = new SettingsHandler();
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
      if (settingsHandler.SettingsFileExists())
      {
        settingsHandler.LoadSettings();
      }
      else 
      {
        settingsHandler.ImportOldSettings();
      }
    }
    #endregion


    #region Methods
    public void OnClosed(object sender, EventArgs e)
    {
      settingsHandler.SaveSettings();
      settingsView.Closed -= OnClosed;
    }


    #endregion
  }
}
