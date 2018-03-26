using System;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace ClangPowerTools
{
    public class ErrorWindow
  {
    #region Members

    private static ErrorListProvider mErrorProvider = null;

    #endregion

    #region Public Methods

    public void Initialize(IServiceProvider aServiceProvider)
    {
      if( null == mErrorProvider )
        mErrorProvider = new ErrorListProvider(aServiceProvider);
    }

    public void Show() => mErrorProvider.Show();

    public void Clear() => mErrorProvider.Tasks.Clear();

    public void SuspendRefresh() => mErrorProvider.SuspendRefresh();

    public void ResumeRefresh() => mErrorProvider.ResumeRefresh();

    public void AddError(TaskError aError) => AddTask(aError);

    public void RemoveErrors(IVsHierarchy aHierarchy) => RemoveTasks(aHierarchy);

    #endregion

    #region Private Methods

    private void AddTask(TaskError aError)
    {
      ErrorTask errorTask = new ErrorTask
      {
        ErrorCategory = aError.Category,
        Document = aError.FilePath,
        Text = aError.Description,
        Line = aError.Line - 1,
        Category = TaskCategory.BuildCompile,
        Priority = TaskPriority.High,
        HierarchyItem = aError.HierarchyItem
      };
      errorTask.Navigate += ErrorTaskNavigate;
      mErrorProvider.Tasks.Add(errorTask);
    }

    private void RemoveTasks(IVsHierarchy aHierarchy)
    {
      for( int i = mErrorProvider.Tasks.Count - 1; i >= 0; --i )
      {
        var errorTask = mErrorProvider.Tasks[i] as ErrorTask;
        aHierarchy.GetCanonicalName( Microsoft.VisualStudio.VSConstants.VSITEMID_ROOT, out string nameInHierarchy );
        errorTask.HierarchyItem.GetCanonicalName( Microsoft.VisualStudio.VSConstants.VSITEMID_ROOT, out string nameErrorTaskHierarchy );
        if( nameInHierarchy == nameErrorTaskHierarchy )
        {
          errorTask.Navigate -= ErrorTaskNavigate;
          mErrorProvider.Tasks.Remove( errorTask );
        }
      }
    }

    private void ErrorTaskNavigate(object sender, EventArgs e)
    {
      ErrorTask objErrorTask = (ErrorTask)sender;
      objErrorTask.Line += 1;
      bool bResult = mErrorProvider.Navigate(objErrorTask, new Guid(EnvDTE.Constants.vsViewKindCode));
      objErrorTask.Line -= 1;
    }

    #endregion

  }
}
