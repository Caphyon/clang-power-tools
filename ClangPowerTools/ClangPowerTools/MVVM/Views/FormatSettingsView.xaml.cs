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
      DataContext = new FormatSettingsViewModel();
    }

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      FallbackStyleComboBox.IsEnabled = StyleComboBox.SelectedItem.ToString() == ComboBoxConstants.kFile;
    }
  }
}
