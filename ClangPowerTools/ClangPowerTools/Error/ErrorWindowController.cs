using System;
using System.Collections.Generic;
using ClangPowerTools.Handlers;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace ClangPowerTools
{
  public class ErrorWindowController : ErrorListProvider
  {

    #region Constructor

    /// <summary>
    /// Instance Constructor
    /// </summary>
    /// <param name="aServiceProvider"></param>
    public ErrorWindowController(IServiceProvider aIServiceProvider) : base(aIServiceProvider)
    {
    }

    #endregion


    #region Public Methods


    public void AddErrors(IEnumerable<TaskErrorModel> aErrors)
    {
      UIUpdater.Invoke(() =>
      {
        SuspendRefresh();

        foreach (TaskErrorModel error in aErrors)
        {
          error.Navigate += ErrorTaskNavigate;
          Tasks.Add(error);
        }

        BringToFront();
        ResumeRefresh();
      });
    }


    public void RemoveErrors(IVsHierarchy aHierarchy)
    {
      UIUpdater.Invoke(() =>
      {
        SuspendRefresh();

        for (int i = Tasks.Count - 1; i >= 0; --i)
        {
          var errorTask = Tasks[i] as ErrorTask;
          aHierarchy.GetCanonicalName(Microsoft.VisualStudio.VSConstants.VSITEMID_ROOT, out string nameInHierarchy);
          errorTask.HierarchyItem.GetCanonicalName(Microsoft.VisualStudio.VSConstants.VSITEMID_ROOT, out string nameErrorTaskHierarchy);
          if (nameInHierarchy == nameErrorTaskHierarchy)
          {
            errorTask.Navigate -= ErrorTaskNavigate;
            Tasks.Remove(errorTask);
          }
        }

        ResumeRefresh();
      });
    }


    public void Clear()
    {
      UIUpdater.Invoke(() =>
      {
        Tasks.Clear();
      });
    }


    public void OnBuildBegin(vsBuildScope Scope, vsBuildAction Action)
    {
      Clear();
    }

    #endregion


    #region Private Methods


    private void ErrorTaskNavigate(object sender, EventArgs e)
    {
      ErrorTask objErrorTask = (ErrorTask)sender;
      objErrorTask.Line += 1;
      bool bResult = Navigate(objErrorTask, new Guid(EnvDTE.Constants.vsViewKindCode));
      objErrorTask.Line -= 1;
    }


    #endregion
  }
}
