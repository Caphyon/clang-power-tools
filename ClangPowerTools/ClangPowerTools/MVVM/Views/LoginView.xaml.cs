using ClangPowerTools.MVVM.Controllers;
using ClangPowerTools.MVVM.WebApi;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ClangPowerTools.MVVM.Views
{
  /// <summary>
  /// Interaction logic for UserControl1.xaml
  /// </summary>
  public partial class LoginView : Window
  {
    private LoginViewModel loginViewModel = new LoginViewModel();
    private AccountController accountController = new AccountController();

    // Login button colors
    private readonly string colorBackgroundEnabled = "#FFBF31";
    private readonly string colorForegroundEnabled = "#000000";
    private readonly string colorBackgroundDisabled = "#BBB6C4";
    private readonly string colorForegroundDisabled = "#707079";

    // Validation messages
    private readonly string invalidEmail = "The email that you have enterd is not valid.";
    private readonly string invalidEmailOrPassword = "The email or password that you have enterd is not valid.";

    // Wrong Email or Password. Please try again. or click Forgot password to reset it.

    public LoginView()
    {
      InitializeComponent();
      DataContext = loginViewModel;
      loginViewModel.InvalidEmail += OnEmailValidation;
      ApiUtility.InitializeApiClient();
    }

    private void OnEmailValidation(object sender, EventArgs e)
    {
      if(loginViewModel.IsInputValid)
      {
        InvalidUserTextBlock.Visibility = Visibility.Hidden;
      }
      else
      {
        InvalidUserTextBlock.Text = invalidEmail;
        InvalidUserTextBlock.Visibility = Visibility.Visible;
      }
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
      if (string.IsNullOrWhiteSpace(loginViewModel.Email) || loginViewModel.IsInputValid == false )
      {
        InvalidUserTextBlock.Text = invalidEmailOrPassword;
        InvalidUserTextBlock.Visibility = Visibility.Visible;
        return;
      }

      SetLoginButtonState(false, colorBackgroundDisabled, colorForegroundDisabled);

      UserModel userModel = new UserModel(loginViewModel.Email, loginViewModel.Password);
      InvalidUserTextBlock.Text = invalidEmailOrPassword;
      InvalidUserTextBlock.Visibility = Visibility.Hidden;

      bool isAccountActive = await accountController.LoginAsync(userModel);
      if (isAccountActive)
      {
        Close();
      }
      else
      {
        SetLoginButtonState(true, colorBackgroundEnabled, colorForegroundEnabled);
        InvalidUserTextBlock.Text = invalidEmailOrPassword;
        InvalidUserTextBlock.Visibility = Visibility.Visible;
      }
    }

    private void SetLoginButtonState(bool isEnabled, string background, string foreground)
    {
      Color colorBackground = (Color)ColorConverter.ConvertFromString(background);
      Color colorForeground = (Color)ColorConverter.ConvertFromString(foreground);

      LoginButton.IsEnabled = isEnabled;
      LoginButton.Background = new SolidColorBrush(colorBackground);
      LoginButton.Foreground = new SolidColorBrush(colorForeground);
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
