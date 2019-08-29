using System.Windows.Controls;

namespace ClangPowerTools.Views
{
  /// <summary>
  /// Interaction logic for FormatSettingsView.xaml
  /// </summary>
  public partial class FormatSettingsView : UserControl
  {
    public FormatSettingsView()
    {
      InitializeComponent();
      DataContext = SettingsViewModelProvider.FormatSettingsViewModel;
    }
  }
}
