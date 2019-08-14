using ClangPowerTools.Views;
using System;

namespace ClangPowerTools
{
  public class SettingsViewModel
  {
    #region Members
    private CPTSettings cptSettings = new CPTSettings();
    private SettingsView settingsView;

    #endregion;

    #region Constructors
    public SettingsViewModel(SettingsView settingsView)
    {
      this.settingsView = settingsView;
      DeserializeSettings();
      settingsView.Closed += OnClosed;
    }

    private void DeserializeSettings()
    {
      if (cptSettings.CheckIfSettingsFileExists())
      {
        cptSettings.DeserializeSettings();
      }
      else if(cptSettings.CheckOldGeneralSettingsExists())
      {
        cptSettings.MapOldSettings();
      }
    }
    #endregion


    #region Methods
    public void OnClosed(object sender, EventArgs e)
    {
      cptSettings.SerializeSettings();
      settingsView.Closed -= OnClosed;
    }

    #endregion
  }
}
