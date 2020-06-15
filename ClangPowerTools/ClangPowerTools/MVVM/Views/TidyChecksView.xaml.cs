using System.Windows;

namespace ClangPowerTools.Views
{
  /// <summary>
  /// Interaction logic for TidyChecksView.xaml
  /// </summary>
  public partial class TidyChecksView : Window
  {
    public TidyChecksView()
    {
      InitializeComponent();
      DataContext = new TidyChecksViewModel(this);
      Owner = SettingsProvider.SettingsView;
    }

    private void ToggleButton_Checked(object sender, RoutedEventArgs e)
    {
      if (TidyChecksListBox.SelectedItems.Count <= 1)
        return;

      foreach (var item in TidyChecksListBox.SelectedItems)
      {
        var model = (TidyCheckModel)item;
        if (!model.IsChecked)
          model.IsChecked = true;
      }
    }

    private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
    {
      if (TidyChecksListBox.SelectedItems.Count <= 1)
        return;

      foreach (var item in TidyChecksListBox.SelectedItems)
      {
        var model = (TidyCheckModel)item;
        if (model.IsChecked)
          model.IsChecked = false;
      }
    }
  }
}
