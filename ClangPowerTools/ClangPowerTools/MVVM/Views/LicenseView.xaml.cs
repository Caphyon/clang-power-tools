using ClangPowerTools.MVVM.Controllers;
using ClangPowerTools.Views;
using System.Windows;
using System.Windows.Navigation;

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
