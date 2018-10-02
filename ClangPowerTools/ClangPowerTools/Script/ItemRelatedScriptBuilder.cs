using ClangPowerTools.Builder;
using EnvDTE;

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
    private IItem mItem;

    #endregion


    #region Constructor


    /// <summary>
    /// Instance constructor
    /// </summary>
    /// <param name="aItem">The current item for which the script will be build</param>
    /// <param name="aSolutionPath">The path of the VS solution</param>
    public ItemRelatedScriptBuilder(IItem aItem) => mItem = aItem;


    #endregion


    #region Methods

    #region Public Methods

    #region IBuilder Implementation


    /// <summary>
    /// Create the script by gathering all the item related parameters from the environment and settings components 
    /// </summary>
    public void Build()
    {
      if (mItem is SelectedProjectItem)
      {
        ProjectItem projectItem = mItem.GetObject() as ProjectItem;
        string containingProject = projectItem.ContainingProject.FullName;
        mScript = $"{mScript} {ScriptConstants.kProject} ''{containingProject}'' " +
          $"{ScriptConstants.kFile} ''{projectItem.Properties.Item("FullPath").Value}'' {ScriptConstants.kActiveConfiguration} " +
          $"''{ProjectConfigurationHandler.GetConfiguration(projectItem.ContainingProject)}|{ProjectConfigurationHandler.GetPlatform(projectItem.ContainingProject)}''";
      }
      else if (mItem is SelectedProject)
      {
        Project project = mItem.GetObject() as Project;
        mScript = $"{mScript} {ScriptConstants.kProject} ''{project.FullName}'' {ScriptConstants.kActiveConfiguration} " +
          $"''{ProjectConfigurationHandler.GetConfiguration(project)}|{ProjectConfigurationHandler.GetPlatform(project)}''";
      }
    }


    /// <summary>
    /// Get the item related script component
    /// </summary>
    /// <returns>Item related script component</returns>
    public string GetResult() => mScript;


    #endregion

    #endregion

    #endregion

  }
}
