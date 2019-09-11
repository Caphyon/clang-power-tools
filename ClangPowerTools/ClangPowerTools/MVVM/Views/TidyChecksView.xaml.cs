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
      DataContext = SettingsProvider.TidyChecksViewModel;
    }
  }
}
