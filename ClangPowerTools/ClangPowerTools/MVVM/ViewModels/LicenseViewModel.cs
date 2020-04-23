using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.MVVM.Controllers;
using ClangPowerTools.MVVM.Views;
using System.Diagnostics;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class LicenseViewModel
  {
    #region Members

    private ICommand commercialLicenseCommand;
    private ICommand personalLicenseCommand;
    private ICommand trialLicenseCommand;
    private ICommand signInCommand;

    private readonly LicenseView licenseView;

    #endregion

    #region Constructor

    public LicenseViewModel() { }

    public LicenseViewModel(LicenseView view)
    {
      licenseView = view;
    }

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

    public ICommand CommercialLicense
    {
      get => commercialLicenseCommand ?? (commercialLicenseCommand = new RelayCommand(() => CommercialLicenceExecute(), () => CanExecute));
    }

    public ICommand PersonalLicense
    {
      get => personalLicenseCommand ?? (personalLicenseCommand = new RelayCommand(() => PersonalLicenceExecute(), () => CanExecute));
    }

    public ICommand TrialLicense
    {
      get => trialLicenseCommand ?? (trialLicenseCommand = new RelayCommand(() => TrialLicenceExecute(), () => CanExecute));
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

    public void TrialLicenceExecute()
    {
      var freeTrialController = new FreeTrialController();
      freeTrialController.Start();
      licenseView.Close();
    }

    #endregion

    #region Private Methods

    public void ShowLoginView()
    {
      licenseView.Close();
      LoginView loginView = new LoginView();
      loginView.ShowDialog();
    }

    #endregion

  }
}
