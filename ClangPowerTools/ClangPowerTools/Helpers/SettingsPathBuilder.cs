using System;
using System.IO;

namespace ClangPowerTools
{
  public class SettingsPathBuilder
  {
    #region Constants

    private const string folderName = "ClangPowerTools";

    #endregion

    #region Methods

    public string GetPath(string aFileName) => Path.Combine(GetFolderPath(), aFileName);

    private string GetFolderPath()
    {
      string folderPath = Path.Combine
        (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), folderName);
      if (!Directory.Exists(folderPath))
        Directory.CreateDirectory(folderPath);
      return folderPath;
    }

    #endregion

  }
}
