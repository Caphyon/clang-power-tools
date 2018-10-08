using ClangPowerTools.Services;
using EnvDTE;
using EnvDTE80;

namespace ClangPowerTools
{
  public static class ActiveWindowProperties
  {
    #region Public Methods

    public static ProjectItem GetProjectItemOfActiveWindow()
    {
      if (VsServiceProvider.TryGetService(typeof(DTE), out object dte))
      {
        var activeWindow = (dte as DTE2).ActiveWindow;
        activeWindow.Activate();
        return activeWindow.ProjectItem;
      }
      return null;
    }

    #endregion

  }
}
