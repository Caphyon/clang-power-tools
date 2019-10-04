using ClangPowerTools.MVVM.Controllers;
using ClangPowerTools.Views;
using System;
using System.Threading.Tasks;

namespace ClangPowerTools
{
  public class SettingsViewModel
  {
    #region Members

    private readonly SettingsHandler settingsHandler = new SettingsHandler();
    private readonly SettingsView settingsView;

    #endregion;

    #region Constructors

    public SettingsViewModel(SettingsView settingsView)
    {
      this.settingsView = settingsView;
      settingsView.Closed += OnClosed;
      SetFooterVisibilityAsync();
    }

    #endregion

    #region Methods

    public void OnClosed(object sender, EventArgs e)
    {
      settingsHandler.SaveSettings();
      settingsView.Closed -= OnClosed;
    }

    public async Task SetFooterVisibilityAsync()
    {
      LicenseController licenseController = new LicenseController();

      if (await licenseController.CheckOnlineLicenseAsync() == true)
      {
        settingsView.SupportGrid.Visibility = System.Windows.Visibility.Visible;
        settingsView.UpgradeGrid.Visibility = System.Windows.Visibility.Hidden;
      }
      else
      {
        settingsView.SupportGrid.Visibility = System.Windows.Visibility.Hidden;
        settingsView.UpgradeGrid.Visibility = System.Windows.Visibility.Visible;
      }

    }

    #endregion
  }
}
