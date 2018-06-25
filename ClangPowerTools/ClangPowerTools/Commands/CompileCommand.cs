﻿using System;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE;
using EnvDTE80;

namespace ClangPowerTools
{
  /// <summary>
  /// Command handler
  /// </summary>
  internal sealed class CompileCommand : ClangCommand
  {
    #region Members

    private bool mExecuteCompile = false;

    #endregion


    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="CompileCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    public CompileCommand(CommandsController aCommandsController, IVsSolution aSolution, 
      DTE2 aDte, AsyncPackage aPackage, Guid aGuid, int aId)
        : base(aCommandsController, aSolution, aDte, aPackage, aGuid, aId)
    {
      var commandService = ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;

      if (null != commandService)
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

    public override void CommandEventsBeforeExecute(string aGuid, int aId, object aCustomIn, object aCustomOut, ref bool aCancelDefault)
    {
      if (false == mGeneralOptions.ClangCompileAfterVsCompile)
        return;

      string commandName = GetCommandName(aGuid, aId);
      if (0 != string.Compare("Build.Compile", commandName))
        return;

      mExecuteCompile = true;
    }

    public override void OnBuildDone(vsBuildScope Scope, vsBuildAction Action)
    {
      if (false == mExecuteCompile)
        return;

      int exitCode = DTEObj.Solution.SolutionBuild.LastBuildInfo;
      if (0 != exitCode)
      {
        // VS compile detected errors and there is not necessary to run clang compile
        mExecuteCompile = false;
        return;
      }

      // Run clang compile after the VS compile succeeded 
      RunClangCompile(new object(), new EventArgs());
      mExecuteCompile = false;
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

      System.Threading.Tasks.Task.Run(() =>
      {
        try
        {
          DocumentsHandler.SaveActiveDocuments((DTE)DTEObj);
          AutomationUtil.SaveDirtyProjects(DTEObj.Solution);

          CollectSelectedItems(ScriptConstants.kAcceptedFileExtensions);
          RunScript(OutputWindowConstants.kComplileCommand);
        }
        catch (Exception exception)
        {
          VsShellUtilities.ShowMessageBox(AsyncPackage, exception.Message, "Error",
            OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

      }).ContinueWith(tsk => mCommandsController.AfterExecute());

    }

    #endregion

  }
}

