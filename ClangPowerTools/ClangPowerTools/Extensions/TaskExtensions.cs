using System;
using System.Threading.Tasks;

namespace ClangPowerTools
{
  /// <summary>
  /// Extension methods for System.Threading.Tasks.Task
  /// </summary>
  public static class TaskExtensions
  {
    #region Public Methods
    /// <summary>
    /// Safely execute the Task without waiting for it to complete before moving to the next line of code; commonly known as "Fire And Forget".
    /// </summary>
    /// <param name="task">Task.</param>
    /// <param name="continueOnCapturedContext">If set to <c>true</c> continue on captured context; this will ensure that the Synchronization Context returns to the calling thread. If set to <c>false</c> continue on a different context; this will allow the Synchronization Context to continue on a different thread</param>
    /// <param name="onException">If an exception is thrown in the Task, <c>onException</c> will execute. If onException is null, the exception will be re-thrown</param>
#pragma warning disable VSTHRD100 // Asynchronous methods should return a Task instead of void
    public static async void SafeFireAndForget(this Task task, bool continueOnCapturedContext = true, Action<Exception> onException = null)
#pragma warning restore VSTHRD100 // Asynchronous methods should return a Task instead of void
    {
      try
      {
#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
        await task.ConfigureAwait(continueOnCapturedContext);
#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks
      }
      catch (Exception ex) when (onException != null)
      {
        onException(ex);
      }
    }
    #endregion
  }
}
