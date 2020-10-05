using System.IO;

namespace ClangPowerTools.MVVM.Models
{
  public class SelectedFileModel
  {
    #region Members

    private const int MAX_FILE_SIZE = 80; // KB

    #endregion


    #region Constructor

    public SelectedFileModel(string path, string pathToShow)
    {
      if (!File.Exists(path))
        return;

      FilePath = path;
      FileSize = new FileInfo(FilePath).Length / 1000;
      FilePathToShow = pathToShow;

      ForgroundColor = FileSize > MAX_FILE_SIZE ? "DarkOrange" : "Black";
      FileSizeAsString = FileSize.ToString() + " KB";
    }

    #endregion


    #region Properties

    public string FilePath { get; private set; }

    public string FilePathToShow { get; private set; }

    public long FileSize { get; private set; }

    public string FileSizeAsString { get; private set; }

    public string ForgroundColor { get; private set; }

    #endregion

  }
}
