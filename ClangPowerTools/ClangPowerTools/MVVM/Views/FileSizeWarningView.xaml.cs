using System.Windows;

namespace ClangPowerTools.MVVM.Views
{
  /// <summary>
  /// Interaction logic for FileSizeWarningView.xaml
  /// </summary>
  public partial class FileSizeWarningView : Window
  {
    public FileSizeWarningView()
    {
      InitializeComponent();
    }

    private void ContinueButton_Click(object sender, RoutedEventArgs e)
    {
      this.Close();
    }
  }
}
