using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.MVVM.Views;
using ClangPowerTools.Views;
using System.Diagnostics;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class LicenseViewModel
  {
    #region Members

    private ICommand commercialLicenseCommand;
    private readonly LicenseView licenseView;

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

    #endregion

    #region Methods

    public void CommercialLicenceExecute()
    {
      Process.Start(new ProcessStartInfo("https://clangpowertools.com/download.html#pricing"));
      LoginView loginView = new LoginView();
      loginView.Show();
      licenseView.Close();
    }

    #endregion

  }
}
