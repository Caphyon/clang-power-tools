using System.Windows;

namespace ClangPowerTools.MVVM.Views
{
  /// <summary>
  /// Interaction logic for DiffWindow.xaml
  /// </summary>
  public partial class DiffWindow : Window
  {
    public DiffWindow(string html)
    {
      DataContext = new DiffViewModel();
      InitializeComponent();
      MyWebBrowser.NavigateToString(html);
    }
  }
}
