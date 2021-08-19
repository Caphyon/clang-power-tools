using System.Windows;

namespace ClangPowerTools.Views
{
  /// <summary>
  /// Interaction logic for TidyChecksView.xaml
  /// </summary>
  public partial class TidyChecksView : Window
  {
    private readonly TidyChecksViewModel viewModel;

    public TidyChecksView()
    {
      InitializeComponent();

      viewModel = new TidyChecksViewModel(this);
      DataContext = viewModel;
      Owner = SettingsProvider.SettingsView;
    }

    private void ToggleButton_Checked(object sender, RoutedEventArgs e)
    {
      viewModel.MultipleStateChange(true);
    }

    private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
    {
      viewModel.MultipleStateChange(false);
    }

    private void OpenDescription(object sender, RoutedEventArgs e)
    {
      viewModel.OpenBrowser();
      //var elementIndex = GetElementIndex(sender as FrameworkElement);
      //llvmSettingsViewModel.DownloadCommand(elementIndex);
    }
  }
}
