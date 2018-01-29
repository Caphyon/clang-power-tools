using EnvDTE;
using EnvDTE80;
using System;
using System.Collections.Generic;
using System.IO;

namespace ClangPowerTools
{
  public class ItemsCollector
  {
    #region Members

    public static readonly List<string> kAcceptedExtensionTypes = new List<string>
      { ".c",
        ".cpp",
        ".cc",
        ".cxx",
        ".c++",
        ".cp",
      };
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

    public void CollectSelectedFiles(DTE2 aDte, ProjectItem aProjectItem)
    {
      try
      {
        // if the command has been given from tab file
        // will be just one file selected
        if (null != aProjectItem)
        {
          AddProjectItem(aProjectItem);
          return;
        }

        // the command has been given from Solution Explorer or toolbar
        Array selectedItems = aDte.ToolWindows.SolutionExplorer.SelectedItems as Array;
        if (null == selectedItems || 0 == selectedItems.Length)
          return;

        foreach (UIHierarchyItem item in selectedItems)
        {
          if (item.Object is Solution)
            GetProjectsFromSolution(item.Object as Solution);

          else if (item.Object is Project)
            AddProject(item.Object as Project);

          else if (item.Object is ProjectItem)
            GetProjectItem(item.Object as ProjectItem);
        }
      }
      catch (Exception)
      {
      }

    }

    public void AddProjectItem(ProjectItem aItem)
    {
      var fileExtension = Path.GetExtension(aItem.Name).ToLower();
      if (!kAcceptedExtensionTypes.Contains(fileExtension))
        return;

      mItems.Add(new SelectedProjectItem(aItem));
    }

    #endregion

    #region Private Methods

    private void GetProjectsFromSolution(Solution aSolution)
    {
      mItems = AutomationUtil.GetAllProjects(mServiceProvider, aSolution);
    }

    private void AddProject(Project aProject) => mItems.Add(new SelectedProject(aProject));

    private void GetProjectItem(ProjectItem aProjectItem)
    {
      // Items that contains projects
      if (null == aProjectItem.ProjectItems)
      {
        if (null != aProjectItem.SubProject)
          AddProject(aProjectItem.SubProject);
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
        AddProjectItem(aProjectItem);
      }
    }

    #endregion

  }
}
