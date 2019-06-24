using ClangPowerTools.MVVM.Controllers;
using ClangPowerTools.MVVM.WebApi;
using System.Diagnostics;
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
      LoginButton.IsEnabled = false;
      UserModel userModel = new UserModel(loginViewModel.Email, loginViewModel.Password);
      InvalidUserTextBlock.Visibility = Visibility.Hidden;


      bool isAccountActive = await accountController.LoginAsync(userModel);
      if(isAccountActive)
      {
        Close();
      }
      else
      {
        LoginButton.IsEnabled = true;
        InvalidUserTextBlock.Visibility = Visibility.Visible;
      }
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
      if (string.IsNullOrWhiteSpace(PasswordTextBox.Password) == false)
      {
        PasswordTextBox.Tag = "True";
      }
      else
      {
        PasswordTextBox.Tag = "False";
      }

      if (DataContext != null)
      {
        loginViewModel.Password = ((PasswordBox)sender).Password;
      }
    }
  }
}
