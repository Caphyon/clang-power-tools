using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;

namespace ClangPowerTools
{
  public class AutomationUtil
  {
    #region Public Methods

    public static List<IItem> GetAllProjects(IServiceProvider aServiceProvider, Solution aSolution)
    {
      List<IItem> list = new List<IItem>();
      Projects projects = aSolution.Projects;

      for (int index = 1; index <= projects.Count; ++index)
      {
        Project project = projects.Item(index);
        if (null == project)
          continue;

        if (project.Kind == EnvDTE.Constants.vsProjectKindSolutionItems)
          list.AddRange(GetSolutionFolderProjects(aServiceProvider, project));
        else if (project.Kind != EnvDTE.Constants.vsProjectKindMisc)
          list.Add(new SelectedProject(project));
      }
      return list;
    }

    public static IVsHierarchy GetProjectHierarchy(IServiceProvider aServiceProvider, Project aProject)
    {
      IVsSolution aSolution = aServiceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
      return VSConstants.S_OK == aSolution.GetProjectOfUniqueName(aProject.UniqueName, out IVsHierarchy hierarchy) ?
        hierarchy : null;
    }

    public static void SaveDirtyProjects(IServiceProvider aServiceProvider, Solution aSolution)
    {
      var projects = GetAllProjects(aServiceProvider, aSolution);
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

    public static bool AreAllUnloadedItems(IEnumerable<IItem> aItems)
    {
      foreach( var item in aItems )
      {
        var projItem = item.GetObject() as ProjectItem;
        if (null == projItem)
          continue;

        var project = projItem.ContainingProject;
        if (null == project)
          continue;

        if (false == IsUnloadedProject(project))
          return false; 
      }

      return true;
    }

    #endregion

    #region Private Methods

    private static List<IItem> GetSolutionFolderProjects(IServiceProvider aServiceProvider, Project aSolutionFolderItem)
    {
      List<IItem> list = new List<IItem>();

      foreach (ProjectItem projectItem in aSolutionFolderItem.ProjectItems)
      {
        Project subProject = projectItem.SubProject;
        if (null == subProject)
          continue;

        if (subProject.Kind == EnvDTE.Constants.vsProjectKindSolutionItems)
          list.AddRange(GetSolutionFolderProjects(aServiceProvider, subProject));
        else
          list.Add(new SelectedProject(subProject));
      }
      return list;
    }

    private static bool IsUnloadedProject(Project aProject)
    {
      return 0 == string.Compare(EnvDTE.Constants.vsProjectKindMisc, aProject.Kind, System.StringComparison.OrdinalIgnoreCase) ||
        0 == string.Compare(EnvDTE.Constants.vsProjectKindUnmodeled, aProject.Kind, System.StringComparison.OrdinalIgnoreCase);
    }



    #endregion

  }
}
