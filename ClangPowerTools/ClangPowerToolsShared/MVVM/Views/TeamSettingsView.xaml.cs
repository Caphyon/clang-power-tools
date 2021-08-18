using System.Windows.Controls;

namespace ClangPowerTools.Views
{
  /// <summary>
  /// Interaction logic for TeamSettingsView.xaml
  /// </summary>
  public partial class TeamSettingsView : UserControl
  {
    public TeamSettingsView()
    {
      InitializeComponent();
      DataContext = new TeamSettingsViewModel();
    }
  }
}
