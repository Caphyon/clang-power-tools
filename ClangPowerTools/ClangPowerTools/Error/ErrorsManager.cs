using EnvDTE80;
using System;
using System.Collections.Generic;
using System.Windows.Interop;
using System.Windows.Threading;

namespace ClangPowerTools
{
  public class ErrorsManager
  {
    #region Members

    private static ErrorWindow mErrorWindow = new ErrorWindow();
    
    #endregion

    #region Constructor

    public ErrorsManager(IServiceProvider aServiceProvider)
    {
      mErrorWindow.Initialize(aServiceProvider);
    }

    #endregion

    #region Public Methods

    public void AddError(TaskError aError)
    {
      if (!String.IsNullOrWhiteSpace(aError.Description))
        mErrorWindow.AddError(aError);
    }

    public void AddErrors(IEnumerable<TaskError> aErrors)
    {
      DispatcherHandler.BeginInvoke(() =>
      {
        mErrorWindow.SuspendRefresh();

        foreach (TaskError error in aErrors)
          mErrorWindow.AddError(error);

        mErrorWindow.Show();
        mErrorWindow.ResumeRefresh();
      });
    }

    public void Clear() => mErrorWindow.Clear();

    #endregion
  }
}
