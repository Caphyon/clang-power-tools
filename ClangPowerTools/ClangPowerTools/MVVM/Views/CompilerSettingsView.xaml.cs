using ClangPowerTools.MVVM.Interfaces;
using System.Windows.Controls;

namespace ClangPowerTools.Views
{
  /// <summary>
  /// Interaction logic for CompilerSettingsView.xaml
  /// </summary>
  public partial class CompilerSettingsView : UserControl, IView
  {
    public CompilerSettingsView()
    {
      InitializeComponent();
      DataContext = new CompilerSettingsViewModel();
      SettingsHandler.RefreshSettingsView += ResetView;
    }

    public void ResetView()
    {
      DataContext = new CompilerSettingsViewModel();
    }
  }
}
