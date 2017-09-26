using EnvDTE80;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Interop;
using System.Windows.Threading;

namespace ClangPowerTools
{
  public class OutputWindowManager
  {
    #region Members

    private DTE2 mDte = null;
    private Dispatcher mDispatcher;

    #endregion

    #region Ctor

    public OutputWindowManager(DTE2 aDte)
    {
      mDte = aDte;
      mDispatcher = HwndSource.FromHwnd((IntPtr)mDte.MainWindow.HWnd).RootVisual.Dispatcher;
    }

    #endregion

    #region Public Methods

    public void AddMessage(string aMessage)
    {
      if (String.IsNullOrWhiteSpace(aMessage))
        return;

      using (OutputWindow outputWindow = new OutputWindow(mDte))
      {
        outputWindow.Show(mDte);
        mDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
        {
          outputWindow.Write(aMessage);
        }));
      }
    }

    #endregion

  }
}
