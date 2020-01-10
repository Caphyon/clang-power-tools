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

    private const int COMMERCIAL_LICENSE_HEIGTH = 460;
    private const int PERSONAL_LICENSE_HEIGTH = 510;

    #endregion

    #region Constructors

    public SettingsViewModel(SettingsView settingsView, bool activeLicense)
    {
      this.settingsView = settingsView;
      settingsView.Closed += OnClosed;
      ActiveLicense = activeLicense;
      Heigth = ActiveLicense ? COMMERCIAL_LICENSE_HEIGTH : PERSONAL_LICENSE_HEIGTH;
    }

    #endregion

    #region Properties

    public bool ActiveLicense { get; private set; }

    public int Heigth { get; set; }

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
