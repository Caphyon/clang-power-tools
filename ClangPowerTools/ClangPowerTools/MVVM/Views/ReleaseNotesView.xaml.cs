using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace ClangPowerTools.MVVM.Views
{
  /// <summary>
  /// Interaction logic for ReleaseNotesView.xaml
  /// </summary>
  public partial class ReleaseNotesView : Window
  {
    public ReleaseNotesView()
    {
      InitializeComponent();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      Process.Start(new ProcessStartInfo("https://clangpowertools.com/download.html#pricing"));
    }

    private void Hyperlink_Navigate(object sender, RequestNavigateEventArgs e)
    {
      Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
      e.Handled = true;
    }
  }
}
