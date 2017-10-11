using EnvDTE;

namespace ClangPowerTools
{
  public class SelectedProject : IItem
  {
    #region Members

    private Project mProject;

    #endregion

    #region Constructor

    public SelectedProject(Project aProject) => mProject = aProject;

    #endregion

    #region IItem implementation

    public string GetName() => mProject.Name;

    public string GetPath() => mProject.FullName;

    public object GetObject() => mProject;

    #endregion

  }
}
