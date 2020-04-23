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
      DataContext = new ReleaseNotesViewModel();
    }
  }
}
