using System;
using System.IO;

namespace ClangPowerTools
{
  public class SettingsPathBuilder
  {
    #region Constants

    private const string kFolderName = "ClangPowerTools";

    #endregion

    #region Methods

    public string GetPath(string aFileName) => Path.Combine(GetFolderPath(), aFileName);

    private string GetFolderPath()
    {
      string folderPath = Path.Combine
        (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), kFolderName);
      if (!Directory.Exists(folderPath))
        Directory.CreateDirectory(folderPath);
      return folderPath;
    }

    #endregion

  }
}
