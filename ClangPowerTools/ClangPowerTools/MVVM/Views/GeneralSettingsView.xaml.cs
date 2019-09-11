using System.Windows.Controls;

namespace ClangPowerTools.Views
{
  /// <summary>
  /// Interaction logic for GeneralSettingsView.xaml
  /// </summary>
  public partial class GeneralSettingsView : UserControl
  {
    public GeneralSettingsView()
    {
      InitializeComponent();
      DataContext = SettingsProvider.GeneralSettingsViewModel;
    }
  }
}
