using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.Views;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class GeneralSettingsViewModel : CommonSettingsFunctionality, INotifyPropertyChanged
  {
    #region Members
    public event PropertyChangedEventHandler PropertyChanged;

    private SettingsHandler settingsHandler = new SettingsHandler();
    private GeneralSettingsModel generalSettingsModel = new GeneralSettingsModel();
    private ICommand logoutCommand;
    private ICommand exportSettingsCommand;
    private ICommand importSettingsCommand;
    private ICommand resetSettingsCommand;
    #endregion


    #region Properties
    public bool CanExecute
    {
      get
      {
        return true;
      }
    }

    public GeneralSettingsModel GeneralSettingsModel
    {
      get
      {
        return generalSettingsModel;
      }
      set
      {
        generalSettingsModel = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("GeneralSettingsModel"));
      }
    }
    #endregion


    #region Commands
    public ICommand LogoutCommand
    {
      get => logoutCommand ?? (logoutCommand = new RelayCommand(() => Logout(), () => CanExecute));
    }

    public ICommand ExportSettingsCommand
    {
      get => exportSettingsCommand ?? (exportSettingsCommand = new RelayCommand(() => ExportSettings(), () => CanExecute));
    }

    public ICommand ImportSettingssCommand
    {
      get => importSettingsCommand ?? (importSettingsCommand = new RelayCommand(() => ImportSettings(), () => CanExecute));
    }

    public ICommand ResetSettingsCommand
    {
      get => resetSettingsCommand ?? (resetSettingsCommand = new RelayCommand(() => ResetSettings(), () => CanExecute));
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
      string path = SaveFile("settings", ".json", "Settings files (.json)|*.json");
      if (string.IsNullOrEmpty(path) == false)
      {
        settingsHandler.SaveSettings(path);
      }
    }

    private void ImportSettings()
    {
      string path = OpenFile("settings", ".json", "Settings files (.json)|*.json");
      if (string.IsNullOrEmpty(path) == false)
      {
        settingsHandler.LoadSettings(path);
      }
    }

    private void ResetSettings()
    {
      settingsHandler.ResetSettings();
    }

    #endregion
  }
}
