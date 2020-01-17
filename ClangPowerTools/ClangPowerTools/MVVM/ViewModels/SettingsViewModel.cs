using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.Views;
using System;
using System.Diagnostics;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class SettingsViewModel
  {
    #region Members

    private ICommand upgradeCommand;

    private readonly SettingsHandler settingsHandler = new SettingsHandler();
    private readonly SettingsView settingsView;

    private const int COMMERCIAL_LICENSE_HEIGTH = 460;
    private const int PERSONAL_LICENSE_HEIGTH = 530;

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

    public bool CanExecute
    {
      get
      {
        return true;
      }
    }

    public ICommand Upgrade
    {
      get => upgradeCommand ?? (upgradeCommand = new RelayCommand(() => UpgradeAction(), () => CanExecute));
    }

    #endregion

    #region Public Methods

    public void OnClosed(object sender, EventArgs e)
    {
      settingsHandler.SaveSettings();
      settingsView.Closed -= OnClosed;
    }

    #endregion

    #region Private Methods

    private void UpgradeAction()
    {
      Process.Start(new ProcessStartInfo("https://clangpowertools.com/download.html"));
    }

    #endregion
  }
}
