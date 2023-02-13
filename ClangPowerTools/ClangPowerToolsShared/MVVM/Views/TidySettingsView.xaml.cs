using ClangPowerTools.MVVM.Interfaces;
using System.Windows.Controls;

namespace ClangPowerTools.Views
{
  /// <summary>
  /// Interaction logic for TidySettingsView.xaml
  /// </summary>
  public partial class TidySettingsView : UserControl, IView
  {
    public TidySettingsView()
    {
      InitializeComponent();
      DataContext = new TidySettingsViewModel();
      SettingsHandler.RefreshSettingsView += ResetView;
    }

    public void ResetView()
    {
      DataContext = new TidySettingsViewModel();
    }

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      SelectPredefinedChecksButton.IsEnabled =
        UseCheckFromComboBox.SelectedItem.ToString() == ClangTidyUseChecksFrom.PredefinedChecks.ToString();

      CustomChecksText.IsReadOnly =
      !(UseCheckFromComboBox.SelectedItem.ToString() == ClangTidyUseChecksFrom.CustomChecks.ToString());
    }
  }
}
