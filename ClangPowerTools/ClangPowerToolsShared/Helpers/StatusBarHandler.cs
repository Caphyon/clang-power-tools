using ClangPowerTools.Handlers;
using ClangPowerTools.Services;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace ClangPowerTools
{
  public class StatusBarHandler
  {
    #region Public Methods


    public static void Text(string aText, int aFreezeStatusBar)
    {
      if (!VsServiceProvider.TryGetService(typeof(SVsStatusbar), out object statusBarService) || null == statusBarService as IVsStatusbar)
        return;

      var statusBar = statusBarService as IVsStatusbar;
      // Make sure the status bar is not frozen
      if (VSConstants.S_OK != statusBar.IsFrozen(out int frozen))
        return;

      UIUpdater.InvokeAsync(() =>
      {
        if (0 != frozen)
          statusBar.FreezeOutput(0);

        // Set the status bar text
        statusBar.SetText(aText);

        // Freeze the status bar.  
        statusBar.FreezeOutput(aFreezeStatusBar);

        // Clear the status bar text.
        if (0 == aFreezeStatusBar)
          statusBar.Clear();

      }).SafeFireAndForget();
    }


    public static void Animation(vsStatusAnimation aAnimation, int aEnableAnimation)
    {
      if (!VsServiceProvider.TryGetService(typeof(SVsStatusbar), out object statusBarService) || null == statusBarService as IVsStatusbar)
        return;

      var statusBar = statusBarService as IVsStatusbar;

      // Use the standard Visual Studio icon for building.  
      object icon = (short)Microsoft.VisualStudio.Shell.Interop.Constants.SBAI_Build;

      UIUpdater.InvokeAsync(() =>
      {
        // Display the icon in the Animation region.  
        statusBar.Animation(aEnableAnimation, ref icon);

      }).SafeFireAndForget();
    }


    public static void Status(string aText, int aFreezeStatusBar, vsStatusAnimation aAnimation, int aEnableAnimation)
    {
      Text(aText, aFreezeStatusBar);
      Animation(aAnimation, aEnableAnimation);
    }

    #endregion

  }
}
