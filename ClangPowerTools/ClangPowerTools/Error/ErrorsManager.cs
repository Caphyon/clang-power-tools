using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell.Interop;

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
      DispatcherHandler.BeginInvoke(() =>
      {
        mErrorWindow.SuspendRefresh();

        if (!String.IsNullOrWhiteSpace(aError.Description))
          mErrorWindow.AddError(aError);

        mErrorWindow.Show();
        mErrorWindow.ResumeRefresh();
      });
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

    public void RemoveErrors(IVsHierarchy aHierarchy)
    {
      DispatcherHandler.BeginInvoke(() =>
      {
        mErrorWindow.SuspendRefresh();
        mErrorWindow.RemoveErrors( aHierarchy );
        mErrorWindow.ResumeRefresh();
      });
    }

    public void Clear()
    {
      DispatcherHandler.BeginInvoke(() =>
      {
        mErrorWindow.Clear();
      });
    }

    #endregion
  }
}
