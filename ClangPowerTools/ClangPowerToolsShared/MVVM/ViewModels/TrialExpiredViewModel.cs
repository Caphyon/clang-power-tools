using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.MVVM.Views;
using System.Diagnostics;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class TrialExpiredViewModel
  {
    #region Members

    private ICommand commercialLicenseCommand;
    private ICommand personalLicenseCommand;
    private ICommand signInCommand;

    private readonly TrialExpiredView trialExpiredView;

    #endregion

    #region Properties

    public bool CanExecute
    {
      get
      {
        return true;
      }
    }

    #region Constructor

    public TrialExpiredViewModel() { }

    public TrialExpiredViewModel(TrialExpiredView view)
    {
      trialExpiredView = view;
    }

    #endregion

    #endregion

    #region Commands

    public ICommand CommercialLicense
    {
      get => commercialLicenseCommand ?? (commercialLicenseCommand = new RelayCommand(() => CommercialLicenceExecute(), () => CanExecute));
    }

    public ICommand PersonalLicense
    {
      get => personalLicenseCommand ?? (personalLicenseCommand = new RelayCommand(() => PersonalLicenceExecute(), () => CanExecute));
    }

    public ICommand SignIn
    {
      get => signInCommand ?? (signInCommand = new RelayCommand(() => PersonalLicenceExecute(), () => CanExecute));
    }

    #endregion

    #region Methods

    public void CommercialLicenceExecute()
    {
      Process.Start(new ProcessStartInfo("https://clangpowertools.com/download.html#pricing"));
      ShowLoginView();
    }

    public void PersonalLicenceExecute()
    {
      ShowLoginView();
    }

    #endregion

    #region Private Methods

    public void ShowLoginView()
    {
      trialExpiredView.Close();
      LoginView loginView = new LoginView();
      loginView.ShowDialog();
    }

    #endregion

  }
}
