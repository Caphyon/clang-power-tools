using Microsoft.VisualStudio.Shell;
using System;
using Task = System.Threading.Tasks.Task;

namespace ClangPowerTools.Handlers
{
  public class UIUpdater
  {
    #region Public Methods

    public static void BeginInvoke(Action aAction)
    {
      aAction.BeginInvoke(aAction.EndInvoke, null);
    }

    public async static Task InvokeAsync(Action aAction)
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      aAction.Invoke();
    }

    #endregion
  }
}
