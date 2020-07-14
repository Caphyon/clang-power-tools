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
      DataContext = new DiffViewModel(html);
      InitializeComponent();
      MyWebBrowser.NavigateToString(html);
    }

    //Empty constructor used for XAML IntelliSense
    public DiffWindow()
    {

    }
  }
}
