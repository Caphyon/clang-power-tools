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
      settingsHandler.LoadSettings();
      settingsView.Closed += OnClosed;
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
