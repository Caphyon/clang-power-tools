using System.IO;

namespace ClangPowerTools
{
  public class PCHCleaner
  {
    #region Members

    private const string kPchExtension = ".clang.pch";

    #endregion

    #region Public Methods

    /// <summary>
    /// Delete all PCH files with ".clang.pch" extension found in folder "aPath" from the disk
    /// </summary>
    /// <param name="aPath">Folder path from where the PCH fils will be deleted</param>
    public void Remove(string aFolderPath)
    {
      if (!Directory.Exists(aFolderPath))
        return;

      var pchPaths = Directory.GetFiles(aFolderPath, $"*{kPchExtension}");
      foreach (var path in pchPaths)
        File.Delete(path);
    }

    #endregion

  }
}
