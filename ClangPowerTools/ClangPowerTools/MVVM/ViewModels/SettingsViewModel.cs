using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.Views;
using System;
using System.Windows.Input;

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
      cptSettings.DeserializeSettings();
      settingsView.Closed += OnClosed;
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
