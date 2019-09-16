using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.MVVM.Views;
using ClangPowerTools.MVVM.WebApi;
using System.Diagnostics;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class TrialExpiredViewModel
  {
    #region Members

    private ICommand createAccountCommand;
    private ICommand signInCommand;

    #endregion

    #region Properties

    public bool CanExecute
    {
      get
      {
        return true;
      }
    }

    #endregion

    #region Commands

    public ICommand CreateAccount
    {
      get => createAccountCommand ?? (createAccountCommand = new RelayCommand(() => CreatAccount(), () => CanExecute));
    }

    public ICommand SignIn
    {
      get => signInCommand ?? (signInCommand = new RelayCommand(() => SignIn(), () => CanExecute));
    }

    #endregion

    #region Private Methods

    private void CreatAccount()
    {
      Process.Start(new ProcessStartInfo(WebApiUrl.signUpUrl));
    }

    private void SignIn()
    {
      var loginView = new LoginView();
      loginView.ShowDialog();
    }

    #endregion
  }
}
;