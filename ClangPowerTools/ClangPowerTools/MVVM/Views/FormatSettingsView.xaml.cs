using ClangPowerTools.MVVM.Interfaces;
using System.Windows.Controls;

namespace ClangPowerTools.Views
{
  /// <summary>
  /// Interaction logic for FormatSettingsView.xaml
  /// </summary>
  public partial class FormatSettingsView : UserControl, IView
  {
    public FormatSettingsView()
    {
      InitializeComponent();
      DataContext = new FormatSettingsViewModel();
      SettingsHandler.ResetSettingsView += ResetView;    
    }

  public void ResetView()
  {
    DataContext = new FormatSettingsViewModel();
  }

  private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      FallbackStyleComboBox.IsEnabled = StyleComboBox.SelectedItem.ToString() == ComboBoxConstants.kFile;
    }

  }
}
