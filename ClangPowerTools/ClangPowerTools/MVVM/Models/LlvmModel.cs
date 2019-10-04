using System.ComponentModel;

namespace ClangPowerTools
{
  public class LlvmModel : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    private bool isDownloading = false;
    private int downloadProgress = 0;

    public string Version { get; set; } = string.Empty;

    public bool IsInstalled { get; set; } = false;

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

    public bool IsSelected { get; set; } = false;

    public bool Cancel { get; set; } = false;

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
  }
}
