using System.Diagnostics;
using System.Windows;

namespace ClangPowerTools.MVVM.Views
{
  /// <summary>
  /// Interaction logic for ReleaseNotesView.xaml
  /// </summary>
  public partial class ReleaseNotesView : Window
  {
    public static bool WasShown { get; set; } = true;

    public ReleaseNotesView(bool wasShown)
    {
      WasShown = wasShown;
      InitializeComponent();
      DataContext = new ReleaseNotesViewModel(this);
    }

    private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
    {
      Process.Start(new ProcessStartInfo("https://youtu.be/9-8jtjExzv4"));
      e.Handled = true;
    }
  }
}
