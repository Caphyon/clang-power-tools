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
    private const int MAX_SIZE_FILE_PATH = 100;

    #endregion


    #region Constructor

    public SelectedFileModel(string path)
    {
      if (!File.Exists(path))
        return;

      filePath = path;
      FileSize = new FileInfo(filePath).Length / 1000;

      ForgroundColor = FileSize > MAX_FILE_SIZE ? "DarkOrange" : "Black";

      FileSizeAsString = FileSize.ToString() + " KB";
    }

    #endregion


    #region Properties

    public string FilePath
    {
      get
      {
        return filePath.Length <= MAX_SIZE_FILE_PATH ? filePath : CreateMiddleEllipsis();
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

    private string CreateMiddleEllipsis()
    {
      while (filePath.Length > MAX_SIZE_FILE_PATH)
        filePath = filePath.Remove(filePath.Length / 2, 1);

      var begin = filePath.Substring(0, filePath.Length / 2);
      begin = begin.Reverse().SubstringAfter("\\").Reverse();

      var end = filePath.Substring(filePath.Length / 2);
      end = end.SubstringAfter("\\");

      return $"{begin}\\...\\{end}";
    }
  }
}
