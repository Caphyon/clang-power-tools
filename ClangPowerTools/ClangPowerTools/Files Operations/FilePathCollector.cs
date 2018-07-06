using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClangPowerTools
{
  public class FilePathCollector
  {
    #region Public Methods

    public IEnumerable<string> Collect(IEnumerable<IItem> aItems)
    => aItems.Select(item => item.GetPath());


    public IEnumerable<string> Collect(Documents aDocuments)
      => aDocuments.Cast<Document>().Select(doc => doc.FullName);


    public string Collect(Document aDocument) => aDocument.FullName;

    #endregion
  }
}
