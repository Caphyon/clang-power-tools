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

    #endregion

    #region Constructor

    public ItemsCollector(List<string> aExtensions = null)
    {
      mAcceptedFileExtensions = aExtensions;
    }

    #endregion 

    #region Properties

    public List<IItem> items { get; private set; } = new List<IItem>();
    public bool haveItems => items.Count != 0;

    #endregion

    #region Public Methods

    public void CollectActiveProjectItem(bool aClangFormatFlag = false)
    {
      try
      {
        DTE vsServiceProvider = VsServiceProvider.TryGetService(typeof(DTE), out object dte) ? (dte as DTE) : null;
        Document activeDocument = vsServiceProvider.ActiveDocument;

        if (activeDocument != null)
        {
          SelectedProjectItem activeProjectItem = new SelectedProjectItem(activeDocument.ProjectItem);
          items.Add(activeProjectItem);
        }
      }
      catch (Exception e)
      {
        throw new Exception(e.Message);
      }
    }

    /// <summary>
    /// Get the name of the active document
    /// </summary>
    public static List<string> GetDocumentsToIgnore()
    {
      List<string> documentsToIgnore = new List<string>();
      DTE vsServiceProvider = VsServiceProvider.TryGetService(typeof(DTE), out object dte) ? (dte as DTE) : null;

      SelectedItems selectedDocuments = vsServiceProvider.SelectedItems;

      for (int i = 1; i <= selectedDocuments.Count; i++)
      {
        documentsToIgnore.Add(selectedDocuments.Item(i).Name);
      }

      return documentsToIgnore;
    }

    /// <summary>
    /// Collect all selected items in the Solution explorer for commands
    /// </summary>
    public void CollectSelectedFiles(ProjectItem aProjectItem, bool aClangFormatFlag = false)
    {
      try
      {
        var dte2 = VsServiceProvider.GetService(typeof(DTE)) as DTE2;
        Array selectedItems = dte2.ToolWindows.SolutionExplorer.SelectedItems as Array;

        if (selectedItems == null || selectedItems.Length == 0)
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
      catch (Exception e)
      {
        throw new Exception(e.Message);
      }
    }


    public void AddProjectItem(ProjectItem aItem)
    {
      if (aItem == null)
        return;

      var fileExtension = Path.GetExtension(aItem.Name).ToLower();
      if (null != mAcceptedFileExtensions && false == mAcceptedFileExtensions.Contains(fileExtension))
        return;

      items.Add(new SelectedProjectItem(aItem));
    }

    #endregion


    #region Private Methods


    private void GetProjectsFromSolution(Solution aSolution)
    {
      items = AutomationUtil.GetAllProjects(aSolution);
    }


    private void AddProject(Project aProject) => items.Add(new SelectedProject(aProject));


    private void GetProjectItem(ProjectItem aProjectItem)
    {
      // Items that contains projects
      if (aProjectItem.ProjectItems == null)
      {
        if (aProjectItem.SubProject != null)
          AddProject(aProjectItem.SubProject);
        return;
      }
      // Folders or filters
      else if (aProjectItem.ProjectItems.Count != 0)
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
        if (projectItem == null)
          continue;

        GetProjectItem(projectItem);
      }
    }


    private void GetProjectItem(Solution aSolution)
    {
      foreach (var item in AutomationUtil.GetAllProjects(aSolution))
      {
        var project = (item as SelectedProject).GetObject() as Project;
        if (project == null)
          continue;

        GetProjectItem(project);
      }
    }

    #endregion
  }
}
