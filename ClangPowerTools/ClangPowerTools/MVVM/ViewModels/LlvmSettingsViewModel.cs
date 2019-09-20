using ClangPowerTools.MVVM.Commands;
using System.ComponentModel;
using System.Net;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class LlvmSettingsViewModel : INotifyPropertyChanged
  {
    #region Members
    public event PropertyChangedEventHandler PropertyChanged;

    private LlvmSettingsModel llvmModel = new LlvmSettingsModel();
    private ICommand dowloadCommand;
    private ICommand deleteCommand;
    private ICommand stopCommand;

    #endregion

    #region Properties
    public LlvmSettingsModel LlvmModel
    {
      get
      {
        return llvmModel;
      }
      set
      {
        llvmModel = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LlvmModel"));
      }
    }


    public bool CanExecute
    {
      get
      {
        return true;
      }
    }
    #endregion

    #region Commands
    public ICommand DownloadCommand
    {
      get => dowloadCommand ?? (dowloadCommand = new RelayCommand(() => DownloadLlvmVersion(), () => CanExecute));
    }

    public ICommand DeleteCommand
    {
      get => deleteCommand ?? (deleteCommand = new RelayCommand(() => DeleteLlvmVersion("8.0.0"), () => CanExecute));
    }

    public ICommand StopCommand
    {
      get => stopCommand ?? (stopCommand = new RelayCommand(() => StopDownload(), () => CanExecute));
    }

    #endregion

    #region Methods
    private void DownloadLlvmVersion()
    {
      using (var client = new WebClient())
      {
        client.DownloadProgressChanged += DownloadProgressChanged;
        SettingsPathBuilder settingsPathBuilder = new SettingsPathBuilder();
        string settingsPath = settingsPathBuilder.GetPath("") + "\\test.exe";

        client.DownloadFileAsync(new System.Uri("https://github.com/llvm/llvm-project/releases/download/llvmorg-8.0.1/LLVM-8.0.1-win32.exe"), settingsPath);
      }
    }

    private void DeleteLlvmVersion(string version)
    {

    }
    
    private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
      llvmModel.DownloadProgress = e.ProgressPercentage;
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LlvmModel"));
    }

    private void StopDownload()
    {

    }
    #endregion
  }
}
