using Microsoft.VisualStudio.Shell;
using System;

namespace ClangPowerTools.Handlers
{
  public class UIUpdater
  {
    #region Public Methods

    public async static void BeginInvoke(Action aAction)
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      aAction.BeginInvoke(aAction.EndInvoke, null);
    }

    public async static void Invoke(Action aAction)
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      aAction.Invoke();
    }

    #endregion

  }
}
