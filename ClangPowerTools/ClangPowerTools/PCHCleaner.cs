using System.Collections.Generic;
using System.IO;

namespace ClangPowerTools
{
  public class PCHCleaner
  {
    #region Public Methods

    public void Clean(string aPath) => DeleteFile(aPath);

    public void Clean(IEnumerable<string> aPaths)
    {
      foreach (var path in aPaths)
        DeleteFile(path);
    }

    #endregion

    #region Private Methods

    private void DeleteFile(string aPath)
    {
      if (File.Exists(aPath))
        File.Delete(aPath);
    }

    #endregion

  }
}
