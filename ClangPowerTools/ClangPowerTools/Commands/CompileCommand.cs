using System;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell.Interop;
using System.Windows.Interop;
using System.Windows.Threading;
using EnvDTE;

namespace ClangPowerTools
{
  /// <summary>
  /// Command handler
  /// </summary>
  internal sealed class CompileCommand : ClangCommand
  {
    #region Members

    private Commands2 mCommand;
    private bool mExecuteCompile = false;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="CompileCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    public CompileCommand(Package aPackage, Guid aGuid, int aId) : base(aPackage, aGuid, aId)
    {
      mCommand = DTEObj.Commands as Commands2;

      if (ServiceProvider.GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
      {
        var menuCommandID = new CommandID(CommandSet, Id);
        var menuCommand = new OleMenuCommand(this.RunClangCompile, menuCommandID);
        menuCommand.BeforeQueryStatus += mCommandsController.QueryCommandHandler;
        menuCommand.Enabled = true;
        commandService.AddCommand(menuCommand);
      }
    }

    #endregion

    #region Public Methods

    public void CommandEventsBeforeExecute(string aGuid, int aId, object aCustomIn, object aCustomOut, ref bool aCancelDefault)
    {
      string commandName = GetCommandName(aGuid, aId);
      if (0 != string.Compare("Build.Compile", commandName))
        return;

      mExecuteCompile = true;
    }

    public void OnBuildDone(vsBuildScope Scope, vsBuildAction Action)
    {
      try
      {
        if (!mExecuteCompile)
          return;

        int exitCode = DTEObj.Solution.SolutionBuild.LastBuildInfo;
        if (0 != exitCode)
        {
          // VS compile detected errors and there is not necessary to run clang compile
          mExecuteCompile = false;
          return; 
        }

        // Run clang compile after the VS compile succeeded 
        var dispatcher = HwndSource.FromHwnd((IntPtr)DTEObj.MainWindow.HWnd).RootVisual.Dispatcher;
        dispatcher.Invoke(DispatcherPriority.Send, new Action(() =>
        {
          RunClangCompile(new object(), new EventArgs());
        }));
      }
      catch (Exception) { }
      finally
      {
        mExecuteCompile = false;
      }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// This function is the callback used to execute the command when the menu item is clicked.
    /// See the constructor to see how the menu item is associated with this function using
    /// OleMenuCommandService service and MenuCommand class.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event args.</param>
    private void RunClangCompile(object sender, EventArgs e)
    {
      if (mCommandsController.Running)
        return;

      mCommandsController.Running = true;
      var task = System.Threading.Tasks.Task.Run(() =>
      {
        try
        {
          AutomationUtil.SaveDirtyFiles(Package, DTEObj.Solution, DTEObj);
          CollectSelectedItems();
          RunScript(OutputWindowConstants.kComplileCommand);
        }
        catch (Exception exception)
        {
          VsShellUtilities.ShowMessageBox(Package, exception.Message, "Error",
            OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
        finally
        {
          mCommandsController.VsCommandIsRunning = false;
        }
      }).ContinueWith(tsk => mCommandsController.AfterExecute());
    }

    private string GetCommandName(string aGuid, int aId)
    {
      if (null == aGuid)
        return "null";

      if (null == mCommand)
        return string.Empty;

      try
      {
        return mCommand.Item(aGuid, aId).Name;
      }
      catch (System.Exception) { }

      return string.Empty;
    }

    #endregion

  }
}

