using ClangPowerToolsShared.MVVM.ViewModels;
using System.Windows.Controls;

namespace ClangPowerTools.Views
{
  /// <summary>
  /// Interaction logic for AboutSettingsView.xaml
  /// </summary>
  public partial class AboutSettingsView : UserControl
  {
    public AboutSettingsView()
    {
      InitializeComponent();
      DataContext = new AboutSettingsViewModel();
    }
  }
}
