using System.Windows;

namespace ClangPowerTools.MVVM.Views
{
  /// <summary>
  /// Interaction logic for DetectedFormatStyleInfo.xaml
  /// </summary>
  public partial class DetectedFormatStyleInfo : Window
  {
    public DetectedFormatStyleInfo(DiffWindow view)
    {
      InitializeComponent();
      Owner = view;
      Owner.IsEnabled = false;
    }

    private void ContinueButton_Click(object sender, RoutedEventArgs e)
    {
      Owner.IsEnabled = true;
      Close();
    }

    private void Window_Closed(object sender, System.EventArgs e)
    {
      Owner.IsEnabled = true;
    }
  }
}
