﻿using System;
using System.ComponentModel.Design;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

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


    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="StopClang"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    public StopClang(CommandsController aCommandsController, IVsSolution aSolution,
      DTE2 aDte, AsyncPackage aPackage, Guid aGuid, int aId)
      : base(aCommandsController, aSolution, aDte, aPackage, aGuid, aId)
    {
      var commandService = ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;

      if (null != commandService)
      {
        var menuCommandID = new CommandID(CommandSet, Id);
        var menuCommand = new OleMenuCommand(this.RunStopClangCommand, menuCommandID);
        menuCommand.BeforeQueryStatus += mCommandsController.QueryCommandHandler;
        menuCommand.Enabled = true;
        commandService.AddCommand(menuCommand);
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
    private void RunStopClangCommand(object sender, EventArgs e)
    {
      mCommandsController.Running = false;

      System.Threading.Tasks.Task.Run(() =>
      {
        try
        {
          mRunningProcesses.Kill();

          string solutionPath = DTEObj.Solution.FullName;
          string solutionFolder = solutionPath.Substring(0, solutionPath.LastIndexOf('\\'));
          mPCHCleaner.Remove(solutionFolder);

          mDirectoriesPath.Clear();
        }
        catch (Exception) { }
      });
    }

    #endregion


  }
}

