using System.Windows;

namespace ClangPowerTools.MVVM.Views
{
  /// <summary>
  /// Interaction logic for Browser.xaml
  /// </summary>
  public partial class BrowserView : Window
  {
    public BrowserView()
    {
      InitializeComponent();
      DataContext = new BrowserViewModel();
    }
  }
}
