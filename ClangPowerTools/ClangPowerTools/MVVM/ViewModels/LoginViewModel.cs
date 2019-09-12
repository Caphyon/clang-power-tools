using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.MVVM.Controllers;
using ClangPowerTools.MVVM.Views;
using ClangPowerTools.MVVM.WebApi;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Task = System.Threading.Tasks.Task;

namespace ClangPowerTools
{
  public class LoginViewModel : INotifyPropertyChanged, IDataErrorInfo
  {
    #region Members

    public event PropertyChangedEventHandler PropertyChanged;
    public event EventHandler<EventArgs> InvalidEmail;

    private AccountController accountController = new AccountController();
    private UserModel userModel = new UserModel();
    private LoginView loginView;

    private ICommand forgotPasswordCommand;
    private ICommand signUpCommand;
    private ICommand signInCommand;

    // Validation messages
    private readonly string invalidEmail = "The email that you have enterd is not valid.";
    private readonly string invalidEmailOrPassword = "The email or password that you have enterd is not valid.";

    // Login button colors
    private readonly string colorBackgroundEnabled = "#FFBF31";
    private readonly string colorForegroundEnabled = "#000000";
    private readonly string colorBackgroundDisabled = "#BBB6C4";
    private readonly string colorForegroundDisabled = "#707079";

    #endregion

    #region Properties

    public string Email
    {
      get { return userModel.email; }
      set
      {
        userModel.email = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Email"));
      }
    }

    public string Password
    {
      get { return userModel.password; }
      set
      {
        userModel.password = value;
      }
    }

    public bool IsInputValid { get; set; } = false;

    public bool CanExecute
    {
      get
      {
        return true;
      }
    }

    #endregion

    #region Constructor

    public LoginViewModel() { }

    public LoginViewModel(LoginView view)
    {
      loginView = view;
    }

    #endregion

    #region Commands

    public ICommand ForgotPassword
    {
      get => forgotPasswordCommand ?? (forgotPasswordCommand = new RelayCommand(() => ForgotPasswordAction(), () => CanExecute));
    }

    public ICommand SignUp
    {
      get => signUpCommand ?? (signUpCommand = new RelayCommand(() => SignUpAction(), () => CanExecute));
    }

    public ICommand SignIn
    {
      get => signInCommand ?? (signInCommand = new RelayCommand(() => SignInActionAsync().SafeFireAndForget(), () => CanExecute));
    }

    #endregion

    #region IDataErrorInfo Interface

    public string Error => null;

    public string this[string name]
    {
      get
      {
        string result = null;

        switch (name)
        {
          case "Email":
            IsInputValid = IsEmailAddressValid(out string errorMessage);
            result = errorMessage;
            InvalidEmail?.Invoke(this, new EventArgs());
            break;
        }
        return result;
      }
    }

    #endregion

    #region Public Methods

    public bool IsEmailAddressValid(out string errorMessage)
    {
      errorMessage = null;
      var validEmailAddress = new EmailAddressAttribute().IsValid(Email);

      if (validEmailAddress == false)
      {
        errorMessage = "Email address is required";
        return false;
      }

      return true;
    }

    public void ForgotPasswordAction()
    {
      Process.Start(new ProcessStartInfo(WebApiUrl.forgotPasswordUrl));
    }

    public void SignUpAction()
    {
      Process.Start(new ProcessStartInfo(WebApiUrl.signUpUrl));
    }

    public async Task SignInActionAsync()
    {
      if (string.IsNullOrWhiteSpace(Email) || IsInputValid == false)
      {
        loginView.InvalidUserTextBlock.Text = invalidEmailOrPassword;
        loginView.InvalidUserTextBlock.Visibility = Visibility.Visible;
        return;
      }

      SetLoginButtonState(false, colorBackgroundDisabled, colorForegroundDisabled);

      UserModel userModel = new UserModel(Email, Password);
      loginView.InvalidUserTextBlock.Text = invalidEmailOrPassword;
      loginView.InvalidUserTextBlock.Visibility = Visibility.Hidden;

      bool isAccountActive = await accountController.LoginAsync(userModel);
      if (isAccountActive)
      {
        loginView.Close();
      }
      else
      {
        SetLoginButtonState(true, colorBackgroundEnabled, colorForegroundEnabled);
        loginView.InvalidUserTextBlock.Text = invalidEmailOrPassword;
        loginView.InvalidUserTextBlock.Visibility = Visibility.Visible;
      }
    }

    #endregion

    #region Private Methods

    private void SetLoginButtonState(bool isEnabled, string background, string foreground)
    {
      Color colorBackground = (Color)ColorConverter.ConvertFromString(background);
      Color colorForeground = (Color)ColorConverter.ConvertFromString(foreground);

      loginView.LoginButton.IsEnabled = isEnabled;
      loginView.LoginButton.Background = new SolidColorBrush(colorBackground);
      loginView.LoginButton.Foreground = new SolidColorBrush(colorForeground);
    }

    #endregion

  }
}
