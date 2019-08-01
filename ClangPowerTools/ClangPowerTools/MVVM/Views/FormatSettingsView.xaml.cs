using System.Windows.Controls;

namespace ClangPowerTools.Views
{
  /// <summary>
  /// Interaction logic for GeneralOptionsView.xaml
  /// </summary>
  public partial class FormatSettingsView : UserControl
  {
    public FormatSettingsView()
    {
      InitializeComponent();
      DataContext = new FormatSettingsViewModel();
    }
  }
}
