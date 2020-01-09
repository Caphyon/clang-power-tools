using ClangPowerTools.Helpers;
using ClangPowerTools.MVVM.LicenseValidation;
using ClangPowerTools.Views;
using System;

namespace ClangPowerTools
{
  public class SettingsViewModel
  {
    #region Members

    private readonly SettingsHandler settingsHandler = new SettingsHandler();
    private readonly SettingsView settingsView;

    #endregion

    #region Constructors

    public SettingsViewModel(SettingsView settingsView, bool activeLicense)
    {
      this.settingsView = settingsView;
      settingsView.Closed += OnClosed;
      ActiveLicense = activeLicense;
    }

    #endregion

    #region Properties

    public bool ActiveLicense { get; private set; }

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
