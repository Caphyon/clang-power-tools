using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerTools
{
  class ActiveProjectItem : IItem
  {
    #region Members

    private Document document;

    #endregion

    #region Constructor 

    public ActiveProjectItem(Document activeDocument) => document = activeDocument;

    #endregion

    #region IItem implementaion

    public string GetName() => document.Name;

    public object GetObject() => document;

    public string GetPath() => document.FullName;

    public void Save() => document.Save("");

    #endregion
  }
}
