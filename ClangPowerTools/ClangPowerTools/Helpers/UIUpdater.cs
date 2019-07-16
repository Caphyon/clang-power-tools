using Microsoft.VisualStudio.Shell;
using System;

namespace ClangPowerTools.Handlers
{
  public class UIUpdater
  {
    #region Public Methods

    public static void BeginInvoke(Action aAction)
    {
      aAction.BeginInvoke(aAction.EndInvoke, null);
    }

    public static void Invoke(Action aAction)
    {
      aAction.Invoke();
    }

    #endregion

  }
}
