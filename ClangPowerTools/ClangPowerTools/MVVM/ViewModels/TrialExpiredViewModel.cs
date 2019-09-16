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

    private readonly TrialExpiredView trialExpiredView;
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

    #region Constructor 

    public TrialExpiredViewModel(TrialExpiredView view)
    {
      trialExpiredView = view;
    }

    #endregion

    #region Commands

    public ICommand CreateAccount
    {
      get => createAccountCommand ?? (createAccountCommand = new RelayCommand(() => CreatAccountAction(), () => CanExecute));
    }

    public ICommand SignIn
    {
      get => signInCommand ?? (signInCommand = new RelayCommand(() => SignInAction(), () => CanExecute));
    }

    #endregion

    #region Private Methods

    private void CreatAccountAction()
    {
      Process.Start(new ProcessStartInfo(WebApiUrl.signUpUrl));
    }

    private void SignInAction()
    {
      trialExpiredView.Close();
      var loginView = new LoginView();
      loginView.ShowDialog();
    }

    #endregion
  }
}
;