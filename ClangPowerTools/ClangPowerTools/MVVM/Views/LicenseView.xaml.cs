using System.Windows;

namespace ClangPowerTools.MVVM.Views
{
  /// <summary>
  /// Interaction logic for License.xaml
  /// </summary>
  public partial class LicenseView : Window
  {
    public LicenseView()
    {
      InitializeComponent();
      DataContext = new LicenseViewModel(this);
    }

  }
}
