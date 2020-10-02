using System.ComponentModel;
using System.IO;

namespace ClangPowerTools.MVVM.Models
{
  public class SelectedFileModel : INotifyPropertyChanged
  {
    #region Members

    public event PropertyChangedEventHandler PropertyChanged;
    private string filePath = string.Empty;

    #endregion


    #region Constructor

    public SelectedFileModel(string path)
    {
      if (!File.Exists(path))
        return;

      FilePath = path;
      FileSize = new FileInfo(FilePath).Length;
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

    public long FileSize { get; private set; } = 0;

    #endregion
  }
}
