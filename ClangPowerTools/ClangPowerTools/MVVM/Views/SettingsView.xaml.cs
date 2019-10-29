using System.Windows;

namespace ClangPowerTools.Views
{
  /// <summary>
  /// Interaction logic for SettingsView.xaml
  /// </summary>
  public partial class SettingsView : Window
  {
    public SettingsView()
    {
      InitializeComponent();
      DataContext = new SettingsViewModel(this);

      SettingsProvider.SettingsView = this;
    }
  }
}
