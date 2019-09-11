using System.Windows.Controls;

namespace ClangPowerTools.Views
{
  /// <summary>
  /// Interaction logic for CompilerSettingsView.xaml
  /// </summary>
  public partial class CompilerSettingsView : UserControl
  {
    public CompilerSettingsView()
    {
      InitializeComponent();
      DataContext = SettingsProvider.CompilerSettingsViewModel;
    }
  }
}
