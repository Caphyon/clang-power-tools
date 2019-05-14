using EnvDTE;

namespace ClangPowerTools
{
  public class CurrentProjectItem : IItem
  {
    #region Members

    private ProjectItem mProjectItem;

    #endregion

    #region Constructor

    public CurrentProjectItem(ProjectItem aProjectItem) => mProjectItem = aProjectItem;

    #endregion

    #region IItem implementation

    public string GetName() => mProjectItem.Name;

    public string GetPath() => mProjectItem.Properties.Item("FullPath").Value.ToString();

    public object GetObject() => mProjectItem;

    public void Save() => mProjectItem.Save("");

    #endregion

  }
}
