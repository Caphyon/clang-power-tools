using EnvDTE;

namespace ClangPowerTools
{
  public class SelectedProjectItem : IItem
  {
    #region Members

    private ProjectItem mProjectItem;

    #endregion

    #region Ctor

    public SelectedProjectItem(ProjectItem aProjectItem) => mProjectItem = aProjectItem;

    #endregion

    #region IItem implementation

    public string GetName() => mProjectItem.Name;

    public string GetPath() => mProjectItem.Properties.Item("FullPath").Value.ToString();

    public object GetObject() => mProjectItem;

    #endregion

  }
}
