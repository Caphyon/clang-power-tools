using System.Windows;

namespace ClangPowerTools.MVVM.Views
{
  /// <summary>
  /// Interaction logic for CMakeBetaWarning.xaml
  /// </summary>
  public partial class CMakeBetaWarning : Window
  {
    public CMakeBetaWarning()
    {
      InitializeComponent();
    }

    private void ContinueButton_Click(object sender, RoutedEventArgs e)
    {
      this.Close();
    }
  }
}
