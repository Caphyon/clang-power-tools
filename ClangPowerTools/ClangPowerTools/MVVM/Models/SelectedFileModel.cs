using System.ComponentModel;
using System.IO;

namespace ClangPowerTools.MVVM.Models
{
  public class SelectedFileModel : INotifyPropertyChanged
  {
    #region Members

    public event PropertyChangedEventHandler PropertyChanged;
    private bool isReadOnly = false;
    private string filePath = string.Empty;

    #endregion


    #region Constructor

    public SelectedFileModel(string path)
    {
      if (!File.Exists(FilePath))
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

    public bool IsReadOnly
    {
      get
      {
        return isReadOnly;
      }
      set
      {
        isReadOnly = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CanEdit"));
      }
    }


    #endregion
  }
}
