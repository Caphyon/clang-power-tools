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

    public void AddError(ScriptError aError) => AddTask(aError, TaskErrorCategory.Error);

    public void AddWarning(ScriptError aWarning) => AddTask(aWarning, TaskErrorCategory.Warning);

    public void AddMessage(ScriptError aMessage) => AddTask(aMessage, TaskErrorCategory.Message);

    #endregion

    #region Private Methods

    private void AddTask(ScriptError aError, TaskErrorCategory aCategory)
    {
      ErrorTask errorTask = new ErrorTask
      {
        ErrorCategory = aCategory,
        HierarchyItem = aError.FileHierarchy,
        Document = aError.FilePath,
        Text = aError.Message,
        Line = aError.Line,
        Column = aError.Column,
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
