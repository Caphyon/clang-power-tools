using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.IO;

namespace ClangPowerTools
{
  public class ItemsCollector
  {
    #region Members

    private readonly List<string> kAcceptedExtensionTypes = new List<string> { ".cpp"};
    private List<IItem> mItems = new List<IItem>();
    private IServiceProvider mServiceProvider;

    #endregion

    #region Constructor

    public ItemsCollector(IServiceProvider aServiceProvider) => mServiceProvider = aServiceProvider;

    #endregion 

    #region Properties

    public List<IItem> GetItems => mItems;
    public bool HaveItems => mItems.Count != 0;

    #endregion

    #region Public Methods

    public void CollectSelectedFiles(DTE2 aDte)
    {
      Array selectedItems = aDte.ToolWindows.SolutionExplorer.SelectedItems as Array;
      if (null == selectedItems || 0 == selectedItems.Length)
        return;

      foreach (UIHierarchyItem item in selectedItems)
      {
        if (item.Object is Solution)
          GetProjectsFromSolution(item.Object as Solution);

        else if (item.Object is Project)
          GetProject(item.Object as Project);

        else if (item.Object is ProjectItem)
          GetProjectItem(item.Object as ProjectItem);
      }
    }

    #endregion

    #region Private Methods

    private void GetProjectsFromSolution(Solution aSolution)
    {
      mItems = AutomationUtil.GetAllProjects(mServiceProvider, aSolution);
    }

    private void GetProject(Project aProject)
    {
      //IVsHierarchy hierarchy = AutomationUtil.GetProjectHierarchy(mServiceProvider, aProject);
      mItems.Add(new SelectedProject(aProject));
    }

    private void GetProjectItem(ProjectItem aProjectItem)
    {
      // Items that contains projects
      if (null == aProjectItem.ProjectItems)
      {
        if (null != aProjectItem.SubProject)
          GetProject(aProjectItem.SubProject);
        return;
      }
      // Folders or filters
      else if (0 != aProjectItem.ProjectItems.Count)
      {
        foreach (ProjectItem projItem in aProjectItem.ProjectItems)
          GetProjectItem(projItem);
      }
      // Files
      else
      {
        GetItem(aProjectItem);
      }
    }

    private void GetItem(ProjectItem aItem)
    {
      if (kAcceptedExtensionTypes.Contains(Path.GetExtension(aItem.Name).ToLower()))
      {
        //IVsHierarchy hierarchy = AutomationUtil.GetProjectHierarchy(mServiceProvider, aItem.ContainingProject);
        mItems.Add(new SelectedProjectItem(aItem));
      }
    }

    #endregion

  }
}
