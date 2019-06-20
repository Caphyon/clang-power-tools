using System.Diagnostics;
using System.Windows;

namespace ClangPowerTools.Views
{
  /// <summary>
  /// Interaction logic for UserControl1.xaml
  /// </summary>
  public partial class LoginView : Window
  {
    public LoginView()
    {
      InitializeComponent();
    }

    private void ForgotPasswordButton_Click(object sender, RoutedEventArgs e)
    {
      Process.Start(new ProcessStartInfo("https://api.clangpowertools.com/api/5d011c6a375f6b5ed9716629/user/forgot-password"));
    }

    private void SingUpButton_Click(object sender, RoutedEventArgs e)
    {
      Process.Start(new ProcessStartInfo("https://api.clangpowertools.com/api/5d011c6a375f6b5ed9716629/user/register"));
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
    }
  }
}
