using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell.Interop;

namespace ClangPowerTools
{
  public class FilePathCollector
  {
    #region members

    private List<string> mFilesPath = new List<string>();

    #endregion

    #region Properties

    public List<string> Files => mFilesPath; 

    #endregion

    #region Public Methods

    //Return the common prefix of all paths stored in the mFilePath list
    public string CommonPrefixPath()
    {
      IEnumerable<string> matchingChars =
            from len in Enumerable.Range(0, mFilesPath.Min(s => s.Length)).Reverse()
            let possibleMatch = mFilesPath.First().Substring(0, len)
            where mFilesPath.All(f => f.StartsWith(possibleMatch))
            select possibleMatch;

      return Path.GetDirectoryName(matchingChars.First());
    }

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
