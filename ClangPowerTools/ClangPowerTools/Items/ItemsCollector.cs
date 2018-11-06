using ClangPowerTools.Services;
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

    private List<string> mAcceptedFileExtensions = new List<string>();
    private List<IItem> mItems = new List<IItem>();

    #endregion

    #region Constructor

    public ItemsCollector(List<string> aExtensions = null)
    {
      mAcceptedFileExtensions = aExtensions;
    }

    #endregion 

    #region Properties

    public List<IItem> GetItems => mItems;
    public bool HaveItems => mItems.Count != 0;

    #endregion

    #region Public Methods

    public void CollectSelectedFiles(ProjectItem aProjectItem, bool aClangFormatFlag = false)
    {
      try
      {
        // if the command has been given from tab file
        // will be just one file selected
        if (null != aProjectItem && false == aClangFormatFlag)
        {
          AddProjectItem(aProjectItem);
          return;
        }

        // the command has been given from Solution Explorer or toolbar
        Array selectedItems = null;
        if (VsServiceProvider.TryGetService(typeof(DTE), out object dte))
          selectedItems = (dte as DTE2).ToolWindows.SolutionExplorer.SelectedItems as Array;

        if (null == selectedItems || 0 == selectedItems.Length)
          return;

        foreach (UIHierarchyItem item in selectedItems)
        {
          if (item.Object is Solution)
          {
            var solution = item.Object as Solution;
            if (aClangFormatFlag)
              GetProjectItem(solution);
            else
              GetProjectsFromSolution(solution);
          }

          else if (item.Object is Project)
          {
            var project = item.Object as Project;
            if (aClangFormatFlag)
              GetProjectItem(project);
            else
              AddProject(project);
          }

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
      if (null == aItem)
        return;

      var fileExtension = Path.GetExtension(aItem.Name).ToLower();
      if (null != mAcceptedFileExtensions && false == mAcceptedFileExtensions.Contains(fileExtension))
        return;

      mItems.Add(new SelectedProjectItem(aItem));
    }

    #endregion


    #region Private Methods


    private void GetProjectsFromSolution(Solution aSolution)
    {
      mItems = AutomationUtil.GetAllProjects(aSolution);
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


    private void GetProjectItem(Project aProject)
    {
      foreach (var item in aProject.ProjectItems)
      {
        var projectItem = item as ProjectItem;
        if (null == projectItem)
          continue;

        GetProjectItem(projectItem);
      }
    }


    private void GetProjectItem(Solution aSolution)
    {
      foreach (var item in AutomationUtil.GetAllProjects(aSolution))
      {
        var project = (item as SelectedProject).GetObject() as Project;
        if (null == project)
          continue;

        GetProjectItem(project);
      }
    }




    #endregion

  }
}
