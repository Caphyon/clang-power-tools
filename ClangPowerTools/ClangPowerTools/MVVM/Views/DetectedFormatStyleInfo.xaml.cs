using System.Windows;

namespace ClangPowerTools.MVVM.Views
{
  /// <summary>
  /// Interaction logic for DetectedFormatStyleInfo.xaml
  /// </summary>
  public partial class DetectedFormatStyleInfo : Window
  {
    private readonly DiffWindow owner;

    public DetectedFormatStyleInfo(DiffWindow view)
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
