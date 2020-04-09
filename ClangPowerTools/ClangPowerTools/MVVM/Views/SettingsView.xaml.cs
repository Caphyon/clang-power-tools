using System.Windows;

namespace ClangPowerTools.Views
{
  /// <summary>
  /// Interaction logic for SettingsView.xaml
  /// </summary>
  public partial class SettingsView : Window
  {
    public SettingsView(bool activeLicense)
    {
      var settingsHandler = new SettingsHandler();
      settingsHandler.LoadSettings();

      InitializeComponent();
      DataContext = new SettingsViewModel(this, activeLicense);

      SettingsProvider.SettingsView = this;
    }
  }
}
