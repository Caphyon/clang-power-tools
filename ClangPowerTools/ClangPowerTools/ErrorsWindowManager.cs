using EnvDTE80;
using System;
using System.Collections.Generic;
using System.Windows.Interop;
using System.Windows.Threading;

namespace ClangPowerTools
{
  public class ErrorsWindowManager
  {
    #region Members

    private ErrorWindow mErrorWindow = new ErrorWindow();
    private Dispatcher mDispatcher;
    
    #endregion

    #region Ctor

    public ErrorsWindowManager(IServiceProvider aServiceProvider, DTE2 aDte)
    {
      mErrorWindow.Initialize(aServiceProvider);
      mDispatcher = HwndSource.FromHwnd((IntPtr)aDte.MainWindow.HWnd).RootVisual.Dispatcher;
    }
    #endregion

    #region Public Methods

    public void AddError(ScriptError aError)
    {
      if (!String.IsNullOrWhiteSpace(aError.ErrorMessage))
        mErrorWindow.AddError(aError);
    }

    public void AddErrors(IEnumerable<ScriptError> aErrors)
    {
      mDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
      {
        mErrorWindow.Clear();
        foreach (ScriptError error in aErrors)
          mErrorWindow.AddError(error);
      }));
    }

    #endregion
  }
}
