using System.Collections.Generic;

namespace ClangPowerTools
{
  public class FilePathCollector
  {
    #region Members

    private List<string> mFilesPath = new List<string>();

    #endregion

    #region Properties

    public List<string> Files => mFilesPath; 

    #endregion

    #region Public Methods

    public void Collect(List<IItem> aItems)
    {
      foreach (var item in aItems)
        Add(item.GetPath());
    }

    #endregion

    #region Private methods

    private void Add(string path) => mFilesPath.Add(path);

    #endregion

  }
}
