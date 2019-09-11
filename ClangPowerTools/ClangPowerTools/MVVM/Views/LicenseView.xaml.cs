using ClangPowerTools.MVVM.Controllers;
using ClangPowerTools.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
      LoginView loginView = new LoginView();
      loginView.Show();
      this.Close();
    }

    private void CommercialLicenceButton_Click(object sender, RoutedEventArgs e)
    {
      Process.Start(new ProcessStartInfo("https://clangpowertools.com/download.html#pricing"));
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
