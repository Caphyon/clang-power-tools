using System.Diagnostics;
using System.Windows;

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
  }
}
