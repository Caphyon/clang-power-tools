using ClangPowerTools;
using ClangPowerTools.MVVM.Command;
using ClangPowerTools.MVVM.Models;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Input;

namespace ClangPowerToolsShared.MVVM.ViewModels
{
  public class AboutSettingsViewModel : CommonSettingsFunctionality
  {
    #region Members
    private readonly SettingsHandler settingsHandler = new SettingsHandler();

    private ICommand exportSettingsCommand;
    private ICommand importSettingsCommand;
    private ICommand resetSettingsCommand;

    private GeneralSettingsModel generalModel;

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion


    #region Constructor

    public AboutSettingsViewModel()
    {
      generalModel = SettingsProvider.GeneralSettingsModel;
    }

    #endregion


    #region Properties

    public GeneralSettingsModel GeneralSettingsModel
    {
      get
      {
        return generalModel;
      }
      set
      {
        generalModel = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("GeneralSettingsModel"));
      }
    }

    public string DisplayMessage
    {
      get { return displayMessage; }
      set
      {
        displayMessage = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DisplayMessage"));
      }
    }
    public bool CanExecute
    {
      get
      {
        return true;
      }
    }

    private string displayMessage;

    #endregion

    public ICommand ExportSettingsCommand
    {
      get => exportSettingsCommand ??= new RelayCommand(() => ExportSettings(), () => CanExecute);
    }

    public ICommand ImportSettingssCommand
    {
      get => importSettingsCommand ??= new RelayCommand(() => ImportSettings(), () => CanExecute);
    }

    public ICommand ResetSettingsCommand
    {
      get => resetSettingsCommand ??= new RelayCommand(() => ResetSettings(), () => CanExecute);
    }

    private void ExportSettings()
    {
      string path = SaveFile("settings", ".json", "Settings files (.json)|*.json");
      if (string.IsNullOrEmpty(path) == false)
      {
        settingsHandler.SaveSettings(path);
        MessageBox.Show("Settings exported.", "Clang Power Tools",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
    }

    private void ImportSettings()
    {
      string path = OpenFile("settings", ".json", "Settings files (.json)|*.json");
      if (string.IsNullOrEmpty(path) == false)
      {
        settingsHandler.LoadSettings(path);
        MessageBox.Show("Settings imported.", "Clang Power Tools",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
    }

    private void ResetSettings()
    {
      settingsHandler.ResetSettings();
      MessageBox.Show("Settings were reset to their default values.", "Clang Power Tools",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
  }
}