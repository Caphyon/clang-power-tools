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
      DataContext = new TidySettingsViewModel();
    }

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      SelectPredefinedChecksButton.IsEnabled =
        UseCheckFromComboBox.SelectedItem.ToString() == ClangTidyUseChecksFrom.PredefinedChecks.ToString();
    }
  }
}
