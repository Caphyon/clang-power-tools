using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.Shell.Interop;

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

    public void Collect(List<Tuple<IItem, IVsHierarchy>> aItems)
    {
      foreach (var item in aItems)
        this.Add(item.Item1.GetPath());
    }

    #endregion

    #region Private methods

    private void Add(string path) => mFilesPath.Add(path);

    #endregion

  }
}
