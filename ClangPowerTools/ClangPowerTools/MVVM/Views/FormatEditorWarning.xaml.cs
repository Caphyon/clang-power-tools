using ClangPowerTools.Helpers;
using System.Windows;

namespace ClangPowerTools.MVVM.Views
{
  /// <summary>
  /// Interaction logic for FormatEditorView.xaml
  /// </summary>
  public partial class FormatEditorWarning : Window
  {
    public FormatEditorWarning()
    {
      InitializeComponent();
    }

    private void DownloadButton(object sender, RoutedEventArgs e)
    {
      FormatEditorUtility.OpenBrowser();
    }
  }
}
