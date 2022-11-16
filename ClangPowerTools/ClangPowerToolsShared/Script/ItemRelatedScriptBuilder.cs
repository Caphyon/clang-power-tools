using ClangPowerTools.Builder;
using ClangPowerTools.Helpers;
using EnvDTE;
using System.Collections.Generic;
using System.Linq;

namespace ClangPowerTools.Script
{
  public class ItemRelatedScriptBuilder : IBuilder<string>
  {
    #region Members

    /// <summary>
    /// The resulted script after the build method
    /// </summary>
    private string mScript = string.Empty;

    /// <summary>
    /// The current item for which the script will be build
    /// </summary>
    private readonly IItem mItem;

    private readonly List<IItem> items;

    private readonly bool jsonCompilationDbActive;

    #endregion


    #region Constructor


    /// <summary>
    /// Instance constructor
    /// </summary>
    /// <param name="aItem">The current item for which the script will be build</param>
    /// <param name="aSolutionPath">The path of the VS solution</param>
    public ItemRelatedScriptBuilder(IItem aItem) => mItem = aItem;

    public ItemRelatedScriptBuilder(IItem aItem, bool jsonCompilationDb) : this(aItem)
    {
      jsonCompilationDbActive = jsonCompilationDb;
    }

    public ItemRelatedScriptBuilder(List<IItem> itemsCollection, bool jsonCompilationDb)
    {
      items = itemsCollection;
      jsonCompilationDbActive = jsonCompilationDb;
    }

    #endregion


    #region Methods

    #region IBuilder Implementation

    /// <summary>
    /// Get the item related script component
    /// </summary>
    /// <returns>Item related script component</returns>
    public string GetResult() => mScript;

    /// <summary>
    /// Create the script by gathering all the item related parameters from the environment and settings components 
    /// CAKE
    /// </summary>
    public void Build()
    {
      if (SolutionInfo.OpenFolderModeActive)
      {
        CreateScriptForOpenFolderProjectItem();
      }
      else
      {
        // Create script for single file / project
        if (mItem != null)
        {
          CreateScriptForSingleFile();
        }
        else if (items != null && items.Count > 0)
        {
          CreateScriptForFilesCollection();
        }
      }
    }

    private void CreateScriptForSingleFile()
    {
      if (mItem is CurrentProjectItem)
      {
        CreateScriptForProjectItem();
      }
      else if (mItem is CurrentProject)
      {
        CreateScriptForProject();
      }
    }

    private void CreateScriptForFilesCollection()
    {
      if (items[0] is CurrentProjectItem)
      {
        CreateScriptForProjectItemCollection();
      }
      else if (items[0] is CurrentProject)
      {
        CreateScriptForProject();
      }
    }

    private void CreateScriptForOpenFolderProjectItem()
    {
      var document = DocumentHandler.GetActiveDocument();

      mScript = $"{mScript} " +
        $"{ScriptConstants.kFile} '{document.FullName}' ";
    }

    private void CreateScriptForProjectItem()
    {
      ProjectItem projectItem = mItem.GetObject() as ProjectItem;
      string containingProject = projectItem.ContainingProject.FullName;

      var filePath = projectItem.Properties.Item("FullPath").Value;
      var configuration = ProjectConfigurationHandler.GetConfiguration(projectItem.ContainingProject);
      var platform = ProjectConfigurationHandler.GetPlatform(projectItem.ContainingProject);

      var projectData = jsonCompilationDbActive ?
        string.Empty : $"{ScriptConstants.kProject} '{containingProject}' ";

      mScript = $"{mScript} {projectData}" +
        $"{ScriptConstants.kFile} '{filePath}' {ScriptConstants.kActiveConfiguration} " +
        $"'{configuration}|{platform}'";
    }

    private void CreateScriptForProjectItemCollection()
    {
      ProjectItem projectItem = items[0].GetObject() as ProjectItem;
      string containingProject = projectItem.ContainingProject.FullName;

      var filesPath = string.Join("','", items.Select(projItem => ((ProjectItem)projItem.GetObject()).Properties.Item("FullPath").Value));
      var configuration = ProjectConfigurationHandler.GetConfiguration(projectItem.ContainingProject);
      var platform = ProjectConfigurationHandler.GetPlatform(projectItem.ContainingProject);

      var projectData = jsonCompilationDbActive ?
        string.Empty : $"{ScriptConstants.kProject} '{containingProject}' ";

      mScript = $"{mScript} {projectData}" +
        $"{ScriptConstants.kFile} ('{filesPath}') {ScriptConstants.kActiveConfiguration} " +
        $"'{configuration}|{platform}'";
    }

    private void CreateScriptForProject()
    {
      Project project = mItem.GetObject() as Project;
      mScript = $"{mScript} {ScriptConstants.kProject} '{project.FullName}' {ScriptConstants.kActiveConfiguration} " +
        $"'{ProjectConfigurationHandler.GetConfiguration(project)}|{ProjectConfigurationHandler.GetPlatform(project)}'";
    }

    #endregion

    #endregion
  }
}
