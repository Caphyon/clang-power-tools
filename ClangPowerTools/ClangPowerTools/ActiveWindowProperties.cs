using EnvDTE;
using EnvDTE80;
using System.IO;

namespace ClangPowerTools
{
  public static class ActiveWindowProperties
  {
    #region Public Methods

    public static ProjectItem GetProjectItemOfActiveWindow(DTE2 aDte)
    {
      var activeWindow = aDte.ActiveWindow;
      activeWindow.Activate();
      return activeWindow.ProjectItem;
    }

    #endregion

  }
}
