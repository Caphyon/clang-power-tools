using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.MVVM.Controllers;
using ClangPowerTools.MVVM.Views;
using System;
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
    private FreeTrialController freeTrialController = new FreeTrialController();

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

    public LicenseViewModel() { }

    public LicenseViewModel(LicenseView view)
    {
      licenseView = view;
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
      freeTrialController.Start("9/12/2018 7:52:51 PM");
      Process.Start(new ProcessStartInfo("https://clangpowertools.com/download.html#pricing"));
      LoginView loginView = new LoginView();
      loginView.Show();
      licenseView.Close();
    }

    public void PersonalLicenceExecute()
    {
      freeTrialController.Start("9/12/2018 7:52:51 PM");
      LoginView loginView = new LoginView();
      loginView.Show();
      licenseView.Close();
    }

    public void TrialLicenceExecute()
    {
      freeTrialController.Start();
      licenseView.Close();
    }

    #endregion

  }
}
