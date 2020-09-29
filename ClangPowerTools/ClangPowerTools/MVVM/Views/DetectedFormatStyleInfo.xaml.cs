using System.Windows;

namespace ClangPowerTools.MVVM.Views
{
  /// <summary>
  /// Interaction logic for DetectedFormatStyleInfo.xaml
  /// </summary>
  public partial class DetectedFormatStyleInfo : Window
  {
    public readonly DetectedStyleInfoViewModel detectedStyleInfoViewModel;

    public DetectedFormatStyleInfo(DiffWindow view, string styleInfo)
    {
      InitializeComponent();
      detectedStyleInfoViewModel = new DetectedStyleInfoViewModel(styleInfo);
      DataContext = detectedStyleInfoViewModel;
      detectedStyleInfoViewModel.DetectedOptions = styleInfo;
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
