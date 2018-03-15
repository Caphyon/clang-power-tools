using Microsoft.VisualStudio.Shell;
using System;

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

    public void AddError(TaskError aError) => AddTask(aError);

    #endregion

    #region Private Methods

    private void AddTask(TaskError aError)
    {
      ErrorTask errorTask = new ErrorTask
      {
        ErrorCategory = aError.Category,
        Document = aError.FilePath,
        Text = aError.Description,
        Line = aError.Line,
        Category = TaskCategory.BuildCompile,
        Priority = TaskPriority.High
      };
      errorTask.Navigate += ErrorTaskNavigate;
      mErrorProvider.Tasks.Add(errorTask);
    }

    private void ErrorTaskNavigate(object sender, EventArgs e)
    {
      ErrorTask objErrorTask = (ErrorTask)sender;
      bool bResult = mErrorProvider.Navigate(objErrorTask, new Guid(EnvDTE.Constants.vsViewKindCode));
    }

    #endregion

  }
}
