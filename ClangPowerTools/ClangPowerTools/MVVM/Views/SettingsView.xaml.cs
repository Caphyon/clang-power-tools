using System.Windows;

namespace ClangPowerTools.Views
{
  /// <summary>
  /// Interaction logic for SettingsView.xaml
  /// </summary>
  public partial class SettingsView : Window
  {
    public SettingsView(bool showFooter)
    {
      var settingsHandler = new SettingsHandler();
      settingsHandler.LoadSettings();

      InitializeComponent();
      DataContext = new SettingsViewModel(this, showFooter);

      SettingsProvider.SettingsView = this;
    }
  }
}
