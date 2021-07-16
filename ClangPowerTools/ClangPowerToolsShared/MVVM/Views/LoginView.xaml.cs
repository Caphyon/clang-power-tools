using System.Windows;
using System.Windows.Controls;

namespace ClangPowerTools.MVVM.Views
{
  /// <summary>
  /// Interaction logic for UserControl1.xaml
  /// </summary>
  public partial class LoginView : Window
  {
    #region Members

    private readonly LoginViewModel loginViewModel;

    #endregion

    #region Methods

    public LoginView()
    {
      InitializeComponent();
      loginViewModel = new LoginViewModel(this);
      DataContext = loginViewModel;
      ApiUtility.InitializeApiClient();
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
      PasswordTextBox.Tag = string.IsNullOrWhiteSpace(PasswordTextBox.Password) ? "False" : "True";

      if (DataContext != null)
      {
        loginViewModel.Password = ((PasswordBox)sender).Password;
      }
    }
    
    #endregion

  }
}
