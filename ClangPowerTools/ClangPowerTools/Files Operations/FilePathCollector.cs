using EnvDTE;
using System.Collections.Generic;

namespace ClangPowerTools
{
  public class FilePathCollector
  {
    #region Public Methods

    public IEnumerable<string> Collect(IEnumerable<IItem> aItems)
    {
      var filesPath = new List<string>();
      foreach (var item in aItems)
        filesPath.Add(item.GetPath());
      return filesPath;
    }

    public IEnumerable<string> Collect(Documents aDocuments)
    {
      var filesPath = new List<string>();
      foreach (Document doc in aDocuments)
        filesPath.Add(doc.FullName);
      return filesPath;
    }

    public string Collect(Document aDocument) => aDocument.FullName;

    #endregion
  }
}
