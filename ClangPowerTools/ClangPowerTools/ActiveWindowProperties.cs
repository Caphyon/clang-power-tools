using EnvDTE;
using EnvDTE80;
using System.IO;

namespace ClangPowerTools
{
  public static class ActiveWindowProperties
  {
    #region Public Methods

    //Solve "Track Active Item in Solution Explorer" set to Off issue
    public static ProjectItem GetProjectItemOfActiveWindow(DTE2 aDte)
    {
      var activeWindow = aDte.ActiveWindow;
      activeWindow.Activate();

      var projectItem = activeWindow.ProjectItem;
      if( null != projectItem )
        SelectContainingProject(aDte, projectItem);
      
      return activeWindow.ProjectItem;
    }

    #endregion

    #region Private Methods

    private static void SelectContainingProject(DTE2 aDte, ProjectItem aProjectItem)
    {
      var solutionPath = aDte.Solution.FullName;
      var solutionName = solutionPath.Substring(solutionPath.LastIndexOf('\\') + 1);
      solutionName = solutionName.Remove(solutionName.LastIndexOf('.'), Path.GetExtension(solutionName).Length);
      var relativePathToSolution = Path.Combine(solutionName, aProjectItem.ContainingProject.Name);

      UIHierarchy uih = aDte.Windows.Item(EnvDTE.Constants.vsWindowKindSolutionExplorer).Object as UIHierarchy;
      uih.GetItem(relativePathToSolution).Select(vsUISelectionType.vsUISelectionTypeSelect);
    }

    #endregion

  }
}
