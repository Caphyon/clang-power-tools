﻿using ClangPowerTools.Handlers;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Windows.Threading;

namespace ClangPowerTools
{
  public class StatusBarHandler
  {
    #region Members

    private static IVsStatusbar mStatusBar = null;

    #endregion

    #region Public Methods

    public static void Initialize(IServiceProvider aServiceProvider)
    {
      if (null == mStatusBar)
        mStatusBar = aServiceProvider.GetService(typeof(SVsStatusbar)) as IVsStatusbar;
    }

    public static void Text(string aText, int aFreezeStatusBar)
    {
      UIUpdater.Invoke(() =>
      {
        // Make sure the status bar is not frozen
        if (VSConstants.S_OK != mStatusBar.IsFrozen(out int frozen))
          return;

        if (0 != frozen)
          mStatusBar.FreezeOutput(0);

        // Set the status bar text
        mStatusBar.SetText(aText);

        // Freeze the status bar.  
        mStatusBar.FreezeOutput(aFreezeStatusBar);

        // Clear the status bar text.
        if (0 == aFreezeStatusBar)
          mStatusBar.Clear();
      });
    }

    public static void Animation(vsStatusAnimation aAnimation, int aEnableAnimation)
    {
      UIUpdater.Invoke(() =>
      {
        // Use the standard Visual Studio icon for building.  
        object icon = (short)Microsoft.VisualStudio.Shell.Interop.Constants.SBAI_Build;

        // Display the icon in the Animation region.  
        mStatusBar.Animation(aEnableAnimation, ref icon);
      });
    }

    public static void Status(string aText, int aFreezeStatusBar, vsStatusAnimation aAnimation, int aEnableAnimation)
    {
      Text(aText, aFreezeStatusBar);
      Animation(aAnimation, aEnableAnimation);
    }

    #endregion

  }
}
