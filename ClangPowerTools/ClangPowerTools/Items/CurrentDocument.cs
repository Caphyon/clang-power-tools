using EnvDTE;

namespace ClangPowerTools.Items
{
  public class CurrentDocument : IItem
  {
    #region Members

    private Document mDocument;

    #endregion

    #region IItem implementation

    public string GetName() => mDocument.Name;

    public object GetObject() => mDocument.Object();

    public string GetPath() => mDocument.FullName;

    public void Save() => mDocument.Save();

    #endregion

  }
}
