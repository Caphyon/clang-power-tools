using System;
using System.Collections.Generic;
using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;

namespace ClangPowerTools
{
  public class ErrorManager
  {
    #region Members

    private ErrorWindow mErrorWindow = new ErrorWindow();

    #endregion

    #region Constructor

    private ErrorManager(IServiceProvider aServiceProvider)
    {
      mErrorWindow.Initialize(aServiceProvider);
    }

    #endregion

    #region Properties

    public static ErrorManager Instance { get; private set; }

    #endregion

    #region Public Methods

    public static void Initialize(IServiceProvider aServiceProvider)
    {
      Instance = new ErrorManager(aServiceProvider);
    }

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
        mErrorWindow.RemoveErrors(aHierarchy);
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

    public void OnBuildBegin(vsBuildScope Scope, vsBuildAction Action)
    {
      Clear();
    }

    #endregion
  }
}
