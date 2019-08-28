﻿using ClangPowerTools.Events;
using ClangPowerTools.Handlers;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;

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

    public void OnErrorDetected(object sender, ErrorDetectedEventArgs e)
    {
      UIUpdater.InvokeAsync(() =>
      {
        SuspendRefresh();

        foreach (TaskErrorModel error in e.ErrorList)
        {
          error.Navigate += ErrorTaskNavigate;
          Tasks.Add(error);
        }

        ResumeRefresh();

        if (e.IsErrorWindowFocused)
        {
          BringToFront();
        }
      }).SafeFireAndForget();
    }

    public void RemoveErrors(IVsHierarchy aHierarchy)
    {
      UIUpdater.InvokeAsync(() =>
      {
        SuspendRefresh();

        for (int i = Tasks.Count - 1; i >= 0; --i)
        {
          var errorTask = Tasks[i] as ErrorTask;
          aHierarchy.GetCanonicalName(Microsoft.VisualStudio.VSConstants.VSITEMID_ROOT, out string nameInHierarchy);

          if (null == errorTask.HierarchyItem)
            return;

          errorTask.HierarchyItem.GetCanonicalName(Microsoft.VisualStudio.VSConstants.VSITEMID_ROOT, out string nameErrorTaskHierarchy);
          if (nameInHierarchy == nameErrorTaskHierarchy)
          {
            errorTask.Navigate -= ErrorTaskNavigate;
            Tasks.Remove(errorTask);
          }
        }
        ResumeRefresh();

      }).SafeFireAndForget();
    }

    public void Clear()
    {
      UIUpdater.InvokeAsync(() =>
      {
        Tasks.Clear();
      }).SafeFireAndForget();
    }

    public void OnClangCommandBegin(object sender, ClearErrorListEventArgs e)
    {
      Clear();
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
