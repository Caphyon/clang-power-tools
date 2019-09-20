using System.Windows.Controls;

namespace ClangPowerTools.Views
{
  /// <summary>
  /// Interaction logic for LlvmSettingsView.xaml
  /// </summary>
  public partial class LlvmSettingsView : UserControl
  {
    public LlvmSettingsView()
    {
      InitializeComponent();
      DataContext = new LlvmSettingsViewModel();
    }
  }
}
