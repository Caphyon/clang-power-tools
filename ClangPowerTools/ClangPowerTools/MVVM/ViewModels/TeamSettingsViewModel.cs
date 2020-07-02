using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.MVVM.LicenseValidation;
using Microsoft.VisualStudio.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class TeamSettingsViewModel : CommonSettingsFunctionality
  {
    #region Members

    private readonly SettingsHandler settingsHandler = new SettingsHandler();

    private ICommand exportSettingsCommand;
    private ICommand importSettingsCommand;
    private ICommand resetSettingsCommand;
    private ICommand uploadSettingsCommand;
    private ICommand downloadSettingsCommand;

    #endregion

    #region Properties

    public bool CanExecute
    {
      get
      {
        return true;
      }
    }

    public bool CanUseCloud
    {
      get
      {
        return SettingsProvider.AccountModel.LicenseType == LicenseType.Commercial;
      }
    }

    #endregion

    #region Commands


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

    public ICommand UploadSettingsCommand
    {
      get => uploadSettingsCommand ??= new RelayCommand(() => UploadCloudSettingsAsync().SafeFireAndForget(), () => CanExecute);
    }

    public ICommand DownloadSettingsCommand
    {
      get => downloadSettingsCommand ??= new RelayCommand(() => DownloadCloudSettingsAsync().SafeFireAndForget(), () => CanExecute);
    }

    #endregion

    #region Methods

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

    private bool ShowWarningMessage(string title, string message)
    {
      DialogResult dialogResult = MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
      return DialogResult.Yes == dialogResult;
    }

    private async Task UploadCloudSettingsAsync()
    {
      if (CanUseCloud == false)
      {
        NoCloudFunctionalityMessage();
        return;
      }

      var settingsApi = new SettingsApi();
      bool cloudSaveExist = await settingsApi.CloudSaveExistsAsync();

      if (cloudSaveExist)
      {
        bool runCommand = ShowWarningMessage("Clang Power Tools", "Cloud settings found.\r\nDo you want to overwrite them?");
        if (runCommand)
        {
          await settingsApi.UploadSettingsAsync();
        }
      }
      else
      {
        await settingsApi.UploadSettingsAsync();
      }
    }

    private async Task DownloadCloudSettingsAsync()
    {
      if (CanUseCloud == false)
      {
        NoCloudFunctionalityMessage();
        return;
      }

      var settingsApi = new SettingsApi();
      bool cloudSaveExist = await settingsApi.CloudSaveExistsAsync();

      if (cloudSaveExist)
      {
        bool runCommand = ShowWarningMessage("Clang Power Tools", "Cloud settings found.\r\nDo you want to overwrite your local settings?");
        if (runCommand)
        {
          await settingsApi.DownloadSettingsAsync();
          MessageBox.Show("Cloud settings downloaded.", "Clang Power Tools",
              MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
      }
      else
      {
        MessageBox.Show("No cloud settings found.", "Clang Power Tools",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
      }
    }

    private void NoCloudFunctionalityMessage()
    {
      MessageBox.Show("Cloud settings can only be used if you have a Commercial License.",
                      "Clang Power Tools", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    #endregion
  }
}
