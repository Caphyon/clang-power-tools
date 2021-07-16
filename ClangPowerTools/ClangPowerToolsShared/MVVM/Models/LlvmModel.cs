using System.ComponentModel;

namespace ClangPowerTools
{
  public class LlvmModel : INotifyPropertyChanged
  {
    #region Members

    public event PropertyChangedEventHandler PropertyChanged;

    private readonly LlvmSettingsModel llvmSettingsModel = new LlvmSettingsModel();
    private bool isDownloading = false;
    private bool isInstalling = false;
    private bool isInstalled = false;
    private bool hasPreinstalledLlvm = false;
    private bool canExecuteCommand = true;
    private int downloadProgress = 0;

    #endregion

    #region Properties

    public string Version
    {
      get
      {
        return llvmSettingsModel.LlvmSelectedVersion;
      }
      set
      {
        llvmSettingsModel.LlvmSelectedVersion = value;
      }
    }


    public bool IsInstalled
    {
      get
      {
        return isInstalled;
      }

      set
      {
        isInstalled = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsInstalled"));
      }
    }

    public bool IsInstalling
    {
      get
      {
        return isInstalling;
      }

      set
      {
        isInstalling = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsInstalling"));
      }
    }

    public bool IsDownloading
    {
      get
      {
        return isDownloading;
      }

      set
      {
        isDownloading = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsDownloading"));
      }
    }

    public bool CanExecuteCommand
    {
      get
      {
        return canExecuteCommand;
      }

      set
      {
        canExecuteCommand = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CanExecuteCommand"));
      }
    }

    public int DownloadProgress
    {
      get
      {
        return downloadProgress;
      }

      set
      {
        downloadProgress = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DownloadProgress"));
      }
    }

    public int MinProgress { get; set; } = 0;

    public int MaxProgress { get; set; } = 100;

    public string PreinstalledLlvmPath
    {
      get
      {
        return llvmSettingsModel.PreinstalledLlvmPath;
      }
      set
      {
        llvmSettingsModel.PreinstalledLlvmPath = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PreinstalledLlvmPath"));
      }
    }

    public bool HasPreinstalledLlvm 
    { 
      get
      {
        return hasPreinstalledLlvm;
      }
      set
      {
        hasPreinstalledLlvm = value;
      }
    }

    #endregion Properties
  }
}
