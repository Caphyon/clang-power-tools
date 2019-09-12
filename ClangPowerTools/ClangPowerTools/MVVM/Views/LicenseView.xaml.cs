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

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
      LoginView loginView = new LoginView();
      loginView.Show();
      this.Close();
    }

    private void FreeLicenceButton_Click(object sender, RoutedEventArgs e)
    {
      LoginView loginView = new LoginView();
      loginView.Show();
      this.Close();
    }

    private void TrialLicenceButton_Click(object sender, RoutedEventArgs e)
    {
      FreeTrialController freeTrialController = new FreeTrialController();
      freeTrialController.Start();
      this.Close();
    }
  }
}
