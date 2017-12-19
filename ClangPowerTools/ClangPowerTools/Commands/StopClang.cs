using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;

namespace ClangPowerTools.Commands
{
  /// <summary>
  /// Command handler
  /// </summary>
  internal sealed class StopClang : ClangCommand
  {
    #region Members

    PCHCleaner mPCHCleaner = new PCHCleaner();

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="StopClang"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    public StopClang(Package aPackage, Guid aGuid, int aId) : base(aPackage, aGuid, aId)
    {
      if (ServiceProvider.GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
      {
        var menuCommandID = new CommandID(CommandSet, Id);
        var menuCommand = new OleMenuCommand(this.MenuItemCallback, menuCommandID);
        menuCommand.BeforeQueryStatus += mCommandsController.QueryCommandHandler;
        menuCommand.Enabled = true;
        commandService.AddCommand(menuCommand);
      }
    }

    /// <summary>
    /// This function is the callback used to execute the command when the menu item is clicked.
    /// See the constructor to see how the menu item is associated with this function using
    /// OleMenuCommandService service and MenuCommand class.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event args.</param>
    private void MenuItemCallback(object sender, EventArgs e)
    {
      mCommandsController.Running = false;
      var task = System.Threading.Tasks.Task.Run(() =>
      {
        mRunningProcesses.KillAll();

        string solutionPath = DTEObj.Solution.FullName;
        string solutionFolder = solutionPath.Substring(0, solutionPath.LastIndexOf('\\'));
        mPCHCleaner.Remove(solutionFolder);

        mDirectoriesPath.Clear();
      });
    }
  }
}
