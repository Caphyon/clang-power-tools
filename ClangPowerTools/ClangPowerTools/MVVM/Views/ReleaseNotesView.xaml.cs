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
      DataContext = new ReleaseNotesViewModel();
    }

  }
}
