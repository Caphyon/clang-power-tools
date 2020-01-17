using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.MVVM.Views;
using ClangPowerTools.Views;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class GeneralSettingsViewModel : CommonSettingsFunctionality, INotifyPropertyChanged
  {
    #region Members

    public event PropertyChangedEventHandler PropertyChanged;

    private readonly SettingsHandler settingsHandler = new SettingsHandler();
    private GeneralSettingsModel generalSettingsModel;

    private ICommand logoutCommand;
    private ICommand exportSettingsCommand;
    private ICommand importSettingsCommand;
    private ICommand resetSettingsCommand;

    #endregion

    #region Constructor

    public GeneralSettingsViewModel()
    {
      var settingsProvider = new SettingsProvider();
      generalSettingsModel = settingsProvider.GetGeneralSettingsModel();
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

      if (File.Exists(path))
        File.Delete(path);

      SettingsProvider.SettingsView.Close();
      LoginView loginView = new LoginView();
      loginView.ShowDialog();
    }

    private void ExportSettings()
    {
      string path = SaveFile("settings", ".json", "Settings files (.json)|*.json");
      if (string.IsNullOrEmpty(path) == false)
      {
        settingsHandler.SaveSettings(path);
        ShowCommandInformationMessage("Information", "Clang Power Tools settings exported at the selected location.");
      }
    }

    private void ImportSettings()
    {
      string path = OpenFile("settings", ".json", "Settings files (.json)|*.json");
      if (string.IsNullOrEmpty(path) == false)
      {
        settingsHandler.LoadSettings(path);
        ShowCommandInformationMessage("Information", "Clang Power Tools settings imported.");
      }
    }

    private void ResetSettings()
    {
      settingsHandler.ResetSettings();
      ShowCommandInformationMessage("Information", "All Clang Power Tools settings were reset to default values.");
    }

    private void ShowCommandInformationMessage(string title, string message)
    {
      MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    #endregion
  }
}
