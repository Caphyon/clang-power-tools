using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;

namespace ClangPowerTools
{
  public class AutomationUtil
  {
    #region Methods

    public static List<Tuple<IItem, IVsHierarchy>> GetAllProjects(IServiceProvider aServiceProvider, Solution aSolution)
    {
      List<Tuple<IItem, IVsHierarchy>> list = new List<Tuple<IItem, IVsHierarchy>>();
      Projects projects = aSolution.Projects;

      for ( int index = 1; index <= projects.Count; ++index)
      {
        Project project = projects.Item(index);
        if (null == project)
          continue;

        if (string.Compare(EnvDTE.Constants.vsProjectKindUnmodeled, project.Kind,
          System.StringComparison.OrdinalIgnoreCase) == 0)
        {
          project = ReloadProject(aServiceProvider, project);
        }

        if (project.Kind == EnvDTE.Constants.vsProjectKindSolutionItems)
          list.AddRange(GetSolutionFolderProjects(aServiceProvider, project));
        else
          list.Add(new Tuple<IItem, IVsHierarchy>( new SelectedProject(project), GetProjectHierarchy(aServiceProvider, project)));
      }
      return list;
    }

    public static Project ReloadProject(IServiceProvider aServiceProvider, Project aProject)
    {
      IVsSolution4 solution = aServiceProvider.GetService(typeof(SVsSolution)) as IVsSolution4;
      IVsHierarchy hierarchy = GetProjectHierarchy(aServiceProvider, aProject);
      if (null == hierarchy)
        return null;

      uint itemId = (uint)VSConstants.VSITEMID.Root;
      if (VSConstants.S_OK != hierarchy.GetGuidProperty(itemId, (int)__VSHPROPID.VSHPROPID_ProjectIDGuid, out Guid projectGuid))
        return null;

      if (VSConstants.S_OK != solution.EnsureProjectIsLoaded(projectGuid, (uint)__VSBSLFLAGS.VSBSLFLAGS_None))
        return null;

      if (VSConstants.S_OK != ((IVsSolution)solution).GetProjectOfGuid(projectGuid, out IVsHierarchy loadedProject))
        return null;

      loadedProject.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_ExtObject, out object objProject);
      return objProject as Project;
    }

    #endregion
    
    #region Helpers

    private static List<Tuple<IItem, IVsHierarchy>> GetSolutionFolderProjects(IServiceProvider aServiceProvider, Project aSolutionFolderItem)
    {
      List<Tuple<IItem, IVsHierarchy>> list = new List<Tuple<IItem, IVsHierarchy>>();
       
      foreach (ProjectItem projectItem in aSolutionFolderItem.ProjectItems)
      {
        Project subProject = projectItem.SubProject;
        if (null == subProject)
          continue;

        if ( 0 == string.Compare(EnvDTE.Constants.vsProjectKindUnmodeled, subProject.Kind,
          System.StringComparison.OrdinalIgnoreCase))
        {
          subProject = ReloadProject(aServiceProvider, subProject);
        }

        if (subProject.Kind == EnvDTE.Constants.vsProjectKindSolutionItems)
          list.AddRange(GetSolutionFolderProjects(aServiceProvider, subProject));
        else
          list.Add(new Tuple<IItem, IVsHierarchy>(new SelectedProject(subProject), GetProjectHierarchy(aServiceProvider, subProject)));
      }
      return list;
    }

    public static IVsHierarchy GetProjectHierarchy(IServiceProvider aServiceProvider, Project aProject)
    {
      IVsSolution aSolution = aServiceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
      return VSConstants.S_OK == aSolution.GetProjectOfUniqueName(aProject.UniqueName, out IVsHierarchy hierarchy) ? 
        hierarchy : null;
    }

    #endregion

  }
}
