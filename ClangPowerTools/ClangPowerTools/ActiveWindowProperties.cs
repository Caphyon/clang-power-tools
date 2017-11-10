using EnvDTE;
using EnvDTE80;

namespace ClangPowerTools
{
  public static class ActiveWindowProperties
  {
    #region Public Methods

    // return true if the method was succesfully executed and false otherwise
    // if the method was succesfully executed the result will be in aProjectItem, otherwise aProjectItem will be null
    public static ProjectItem GetProjectItemOfActiveWindow(DTE2 aDte)
    {
      var activeWindow = aDte.ActiveWindow;
      activeWindow.Activate();
      return activeWindow.ProjectItem;
    }
    
    #endregion

  }
}
