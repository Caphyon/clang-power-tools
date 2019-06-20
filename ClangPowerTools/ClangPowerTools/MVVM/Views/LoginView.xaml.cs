using ClangPowerTools.MVVM.Controllers;
using ClangPowerTools.MVVM.WebApi;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ClangPowerTools.Views
{
  /// <summary>
  /// Interaction logic for UserControl1.xaml
  /// </summary>
  public partial class LoginView : Window
  {
    private LoginViewModel loginViewModel = new LoginViewModel();
    private AccountController accountController = new AccountController();

    public LoginView()
    {
      InitializeComponent();
      DataContext = loginViewModel;
      ApiUtility.InitializeApiClient();
    }

    private void ForgotPasswordButton_Click(object sender, RoutedEventArgs e)
    {
      Process.Start(new ProcessStartInfo(WebApiUrl.forgotPasswordUrl));
    }

    private void SingUpButton_Click(object sender, RoutedEventArgs e)
    {
      Process.Start(new ProcessStartInfo(WebApiUrl.signUpUrl));
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
      UserModel userModel = new UserModel(loginViewModel.Email, loginViewModel.Password);
      loginViewModel.Password = string.Empty;

      bool isAccountActive = await accountController.LoginAsync(userModel);
      if(isAccountActive)
      {
        Close();
      }
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
      if (DataContext != null)
      {
        loginViewModel.Password = ((PasswordBox)sender).Password;
      }
    }
  }
}
