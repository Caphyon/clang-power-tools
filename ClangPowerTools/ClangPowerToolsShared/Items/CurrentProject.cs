﻿using EnvDTE;

namespace ClangPowerTools
{
  public class CurrentProject : IItem
  {
    #region Members

    private Project mProject;

    #endregion

    #region Constructor

    public CurrentProject(Project aProject) => mProject = aProject;

    #endregion

    #region IItem implementation

    public string GetName() => mProject.FullName.Substring(mProject.FullName.LastIndexOf('\\') + 1);

    public string GetPath() => mProject.FullName;

    public object GetObject() => mProject;

    public void Save() => mProject.Save(mProject.FullName);

    #endregion

  }
}
