using System;
using System.Collections.Generic;
using ClangPowerTools.Handlers;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace ClangPowerTools
{
  public class ErrorWindow : ErrorListProvider
  {

    #region Members

    //private static ErrorListProvider mErrorListProvider = null;

    #endregion


    #region Constructor

    /// <summary>
    /// Instance Constructor
    /// </summary>
    /// <param name="aServiceProvider"></param>
    public ErrorWindow(IServiceProvider aIServiceProvider) : base(aIServiceProvider)
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
          ErrorTask errorTask = new ErrorTask
          {
            ErrorCategory = error.Category,
            Document = error.FilePath,
            Text = error.Description,
            Line = error.Line - 1,
            Column = error.Column,
            Category = TaskCategory.BuildCompile,
            Priority = TaskPriority.High,
            HierarchyItem = error.HierarchyItem
          };
          errorTask.Navigate += ErrorTaskNavigate;
          Tasks.Add(errorTask);
        }

        BringToFront();
        ResumeRefresh();
      });
    }


    private void ErrorTaskNavigate(object sender, EventArgs e)
    {
      ErrorTask objErrorTask = (ErrorTask)sender;
      objErrorTask.Line += 1;
      bool bResult = Navigate(objErrorTask, new Guid(EnvDTE.Constants.vsViewKindCode));
      objErrorTask.Line -= 1;
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
  }
}
