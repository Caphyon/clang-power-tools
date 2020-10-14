using Microsoft.VisualStudio.PlatformUI;

namespace ClangPowerTools.Views
{
  /// <summary>
  /// Interaction logic for SettingsView.xaml
  /// </summary>
  public partial class SettingsView : DialogWindow
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
