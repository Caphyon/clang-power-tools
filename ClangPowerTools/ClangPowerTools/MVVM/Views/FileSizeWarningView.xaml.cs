using System.Windows;

namespace ClangPowerTools.MVVM.Views
{
  /// <summary>
  /// Interaction logic for FileSizeWarningView.xaml
  /// </summary>
  public partial class FileSizeWarningView : Window
  {
    private readonly DetectFormatStyleMenuView owner;

    public FileSizeWarningView(DetectFormatStyleMenuView view)
    {
      InitializeComponent();
      owner = view;
      owner.IsEnabled = false;
    }

    private void ContinueButton_Click(object sender, RoutedEventArgs e)
    {
      owner.IsEnabled = true;
      this.Close();
    }

    private void Window_Closed(object sender, System.EventArgs e)
    {
      owner.IsEnabled = true;
    }
  }
}
