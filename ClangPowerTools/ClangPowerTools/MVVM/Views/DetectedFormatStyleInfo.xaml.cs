using System.Windows;

namespace ClangPowerTools.MVVM.Views
{
  /// <summary>
  /// Interaction logic for DetectedFormatStyleInfo.xaml
  /// </summary>
  public partial class DetectedFormatStyleInfo : Window
  {
    public DetectedFormatStyleInfo()
    {
      InitializeComponent();
    }

    private void ContinueButton_Click(object sender, RoutedEventArgs e)
    {
      this.Close();
    }
  }
}
