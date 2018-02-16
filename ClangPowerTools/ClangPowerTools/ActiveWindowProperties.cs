using EnvDTE;
using EnvDTE80;
using System;

namespace ClangPowerTools
{
  public static class ActiveWindowProperties
  {
    #region Public Methods

    public static ProjectItem GetProjectItemOfActiveWindow(DTE2 aDte)
    {
      try
      {
        var activeWindow = aDte.ActiveWindow;
        activeWindow.Activate();
        return activeWindow.ProjectItem;
      }
      catch (Exception)
      {
      }
      return null;
    }

    #endregion

  }
}
