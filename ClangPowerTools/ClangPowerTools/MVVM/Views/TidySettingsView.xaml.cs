using System.Windows.Controls;

namespace ClangPowerTools.Views
{
  /// <summary>
  /// Interaction logic for TidySettingsView.xaml
  /// </summary>
  public partial class TidySettingsView : UserControl
  {
    public TidySettingsView()
    {
      InitializeComponent();
      DataContext = SettingsProvider.TidySettingsViewModel;
    }
  }
}
