using System.ComponentModel;

namespace ClangPowerTools
{
  public class LlvmModel : INotifyPropertyChanged
  {
    #region Members
    public event PropertyChangedEventHandler PropertyChanged;

    private bool isDownloading = false;
    private bool isInstalling = false;
    private bool isInstalled = false;
    private int downloadProgress = 0;
    #endregion

    #region Properties
    public string Version { get; set; } = string.Empty;

    public bool IsSelected { get; set; } = false;

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
    #endregion Properties
  }
}
