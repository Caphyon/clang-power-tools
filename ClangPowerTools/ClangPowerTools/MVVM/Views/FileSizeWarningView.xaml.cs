using System.Windows;

namespace ClangPowerTools.MVVM.Views
{
  /// <summary>
  /// Interaction logic for FileSizeWarningView.xaml
  /// </summary>
  public partial class FileSizeWarningView : Window
  {
    public FileSizeWarningView(DetectFormatStyleMenuView view)
    {
      InitializeComponent();
      Owner = view;
      Owner.IsEnabled = false;
    }

    private void ContinueButton_Click(object sender, RoutedEventArgs e)
    {
      Owner.IsEnabled = true;
      this.Close();
    }

    private void Window_Closed(object sender, System.EventArgs e)
    {
      Owner.IsEnabled = true;
    }
  }
}
