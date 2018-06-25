using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.Generic;

namespace ClangPowerTools
{
  public class AutomationUtil
  {

    #region Public Methods

    public static List<IItem> GetAllProjects(Solution aSolution)
    {
      List<IItem> list = new List<IItem>();
      Projects projects = aSolution.Projects;

      for (int index = 1; index <= projects.Count; ++index)
      {
        Project project = projects.Item(index);
        if (null == project)
          continue;

        if (project.Kind == EnvDTE.Constants.vsProjectKindSolutionItems)
          list.AddRange(GetSolutionFolderProjects(project));
        else if (project.Kind != EnvDTE.Constants.vsProjectKindMisc)
          list.Add(new SelectedProject(project));
      }
      return list;
    }

    public static IVsHierarchy GetProjectHierarchy(IVsSolution aSolution, Project aProject)
    {
      return 
        VSConstants.S_OK == aSolution.GetProjectOfUniqueName(aProject.UniqueName, out IVsHierarchy hierarchy) ?
          hierarchy : null;
    }

    public static IVsHierarchy GetItemHierarchy(IVsSolution aSolution, IItem aItem )
    {
      Project project = null;
      if( aItem is SelectedProjectItem )
      {
        var projectItem = aItem.GetObject() as ProjectItem;
        project = projectItem.ContainingProject;
      }
      else if( aItem is SelectedProject )
      {
        project = aItem.GetObject() as Project;
      }
      if( project != null )
      {
        return GetProjectHierarchy(aSolution, project );
      }
      return null;
    }

    public static void SaveDirtyProjects(Solution aSolution)
    {
      var projects = GetAllProjects(aSolution);
      if (null == projects)
        return;

      foreach (var proj in projects)
      {
        var project = proj.GetObject() as Project;
        if (null == project)
          continue;

        if (true == project.IsDirty)
          project.Save(project.FullName);
      }
    }

    public static bool ContainLoadedItems(IEnumerable<IItem> aItems)
    {
      foreach( var item in aItems )
      {
        var projItem = item.GetObject() as ProjectItem;
        if (null == projItem)
          return true;

        var project = projItem.ContainingProject;
        if (null == project)
          return true;

        if (true == IsLoadedProject(project))
          return true; 
      }

      return false;
    }

    #endregion

    #region Private Methods

    private static List<IItem> GetSolutionFolderProjects(Project aSolutionFolderItem)
    {
      List<IItem> list = new List<IItem>();

      foreach (ProjectItem projectItem in aSolutionFolderItem.ProjectItems)
      {
        Project subProject = projectItem.SubProject;
        if (null == subProject)
          continue;

        if (subProject.Kind == EnvDTE.Constants.vsProjectKindSolutionItems)
          list.AddRange(GetSolutionFolderProjects(subProject));
        else if (subProject.Kind != EnvDTE.Constants.vsProjectKindMisc)
          list.Add(new SelectedProject(subProject));
      }
      return list;
    }

    private static bool IsLoadedProject(Project aProject)
    {
      return 0 != string.Compare(EnvDTE.Constants.vsProjectKindMisc, aProject.Kind, System.StringComparison.OrdinalIgnoreCase) &&
        0 != string.Compare(EnvDTE.Constants.vsProjectKindUnmodeled, aProject.Kind, System.StringComparison.OrdinalIgnoreCase);
    }

    #endregion

  }
}
