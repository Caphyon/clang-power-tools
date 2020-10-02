using System.ComponentModel;
using System.IO;

namespace ClangPowerTools.MVVM.Models
{
  public class SelectedFileModel : INotifyPropertyChanged
  {
    #region Members

    public event PropertyChangedEventHandler PropertyChanged;
    private string filePath = string.Empty;
    private const int MAX_FILE_SIZE = 500; // KB

    #endregion


    #region Constructor

    public SelectedFileModel(string path)
    {
      if (!File.Exists(path))
        return;

      FilePath = path;
      FileSize = new FileInfo(FilePath).Length / 1000;

      ForgroundColor = FileSize > MAX_FILE_SIZE ? "DarkOrange" : "Black";

      FileSizeAsString = FileSize.ToString() + " KB";
    }

    #endregion


    #region Properties

    public string FilePath
    {
      get
      {
        return filePath;
      }
      set
      {
        filePath = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FilePath"));
      }
    }

    public long FileSize { get; private set; }

    public string FileSizeAsString { get; private set; }

    public string ForgroundColor { get; private set; }

    #endregion
  }
}
