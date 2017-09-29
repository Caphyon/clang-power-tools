using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;

namespace ClangPowerTools
{
  public class AutomationUtil
  {
    #region Public methods

    public static List<Tuple<IItem, IVsHierarchy>> GetAllProjects(IServiceProvider aServiceProvider, Solution aSolution)
    {
      List<Tuple<IItem, IVsHierarchy>> list = new List<Tuple<IItem, IVsHierarchy>>();
      Projects projects = aSolution.Projects;

      for ( int index = 1; index <= projects.Count; ++index)
      {
        Project project = projects.Item(index);
        if (null == project)
          continue;

        if (project.Kind == EnvDTE.Constants.vsProjectKindSolutionItems)
          list.AddRange(GetSolutionFolderProjects(aServiceProvider, project));
        else
          list.Add(new Tuple<IItem, IVsHierarchy>( new SelectedProject(project), GetProjectHierarchy(aServiceProvider, project)));
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

    #region Helpers

    private static List<Tuple<IItem, IVsHierarchy>> GetSolutionFolderProjects(IServiceProvider aServiceProvider, Project aSolutionFolderItem)
    {
      List<Tuple<IItem, IVsHierarchy>> list = new List<Tuple<IItem, IVsHierarchy>>();
       
      foreach (ProjectItem projectItem in aSolutionFolderItem.ProjectItems)
      {
        Project subProject = projectItem.SubProject;
        if (null == subProject)
          continue;

        if (subProject.Kind == EnvDTE.Constants.vsProjectKindSolutionItems)
          list.AddRange(GetSolutionFolderProjects(aServiceProvider, subProject));
        else
          list.Add(new Tuple<IItem, IVsHierarchy>(new SelectedProject(subProject), GetProjectHierarchy(aServiceProvider, subProject)));
      }
      return list;
    }

    #endregion

  }
}
