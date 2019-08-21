using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.Views;
using System.IO;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class GeneralSettingsViewModel : CommonSettingsFunctionality
  {
    #region Members
    private SettingsHandler cptSettings = new SettingsHandler();

    private ICommand logoutCommand;
    private ICommand exportSettingsCommand;
    private ICommand importSettingsCommand;
    #endregion

    #region Commands
    public ICommand LogoutCommand
    {
      get => logoutCommand ?? (logoutCommand = new RelayCommand(() => Logout(), () => CanExecute));
    }

    public ICommand ExportSettingsCommand
    {
      get => logoutCommand ?? (exportSettingsCommand = new RelayCommand(() => ExportSettings(), () => CanExecute));
    }

    public ICommand ImportSettingssCommand
    {
      get => logoutCommand ?? (importSettingsCommand = new RelayCommand(() => ImportSettings(), () => CanExecute));
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


    #region Methods
    private void Logout()
    {
      var settingsPathBuilder = new SettingsPathBuilder();
      string path = settingsPathBuilder.GetPath("ctpjwt");

      if (File.Exists(path) == true)
      {
        File.Delete(path);
      }

      LoginView loginView = new LoginView();
      loginView.ShowDialog();
    }

    private void ExportSettings()
    {
     // cptSettings.
    }

    private void ImportSettings()
    {

    }

    #endregion
  }
}
