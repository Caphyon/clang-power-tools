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
      TidyChecksViewModel tidyChecksViewModel = SettingsViewModelProvider.TidyChecksViewModel;
      tidyChecksViewModel.TidyChecksView = this;
      DataContext = SettingsViewModelProvider.TidyChecksViewModel;
    }
  }
}
