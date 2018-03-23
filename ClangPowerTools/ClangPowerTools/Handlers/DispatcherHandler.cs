using EnvDTE80;
using System;
using System.Windows.Interop;
using System.Windows.Threading;

namespace ClangPowerTools
{
  public class DispatcherHandler
  {

    #region Members

    private static Dispatcher mDispatcher = null;

    #endregion

    #region Public Methods

    public static void Initialize(DTE2 aDte)
    {
      mDispatcher = HwndSource.FromHwnd((IntPtr)aDte.MainWindow.HWnd).RootVisual.Dispatcher;
    }

    public static void BeginInvoke(Action aAction)
    {
      mDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
      {
        aAction.Invoke();
      }));
    }

    public static void Invoke(Action aAction)
    {
      mDispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
      {
        aAction.Invoke();
      }));
    }

    #endregion

  }
}
