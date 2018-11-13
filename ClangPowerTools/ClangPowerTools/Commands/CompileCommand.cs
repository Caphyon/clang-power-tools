﻿using ClangPowerTools.Output;
using ClangPowerTools.Services;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;

namespace ClangPowerTools.Commands
{
  /// <summary>
  /// Command handler
  /// </summary>
  internal sealed class CompileCommand : ClangCommand
  {
    #region Members

    private bool mExecuteCompile = false;

    #endregion


    #region Properties


    /// <summary>
    /// Gets the instance of the command.
    /// </summary>
    public static CompileCommand Instance
    {
      get;
      private set;
    }


    #endregion


    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="CompileCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    private CompileCommand(OleMenuCommandService aCommandService, CommandsController aCommandsController, 
      ErrorWindowController aErrorWindow, OutputWindowController aOutputWindow, AsyncPackage aPackage, Guid aGuid, int aId)
        : base(aCommandsController, aErrorWindow, aOutputWindow, aPackage, aGuid, aId)
    {
      if (null != aCommandService)
      {
        var menuCommandID = new CommandID(CommandSet, Id);
        var menuCommand = new OleMenuCommand(mCommandsController.Execute, menuCommandID);
        menuCommand.BeforeQueryStatus += mCommandsController.OnBeforeClangCommand;
        menuCommand.Enabled = true;
        aCommandService.AddCommand(menuCommand);
      }
    }

    #endregion


    #region Public Methods


    /// <summary>
    /// Initializes the singleton instance of the command.
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    public static async System.Threading.Tasks.Task InitializeAsync(CommandsController aCommandsController, 
      ErrorWindowController aErrorWindow, OutputWindowController aOutputWindow, AsyncPackage aPackage, Guid aGuid, int aId)
    {
      // Switch to the main thread - the call to AddCommand in CompileCommand's constructor requires
      // the UI thread.
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(aPackage.DisposalToken);

      OleMenuCommandService commandService = await aPackage.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
      Instance = new CompileCommand(commandService, aCommandsController, aErrorWindow, aOutputWindow, aPackage, aGuid, aId);
    }


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

      var exitCode = int.MaxValue;
      if (VsServiceProvider.TryGetService(typeof(DTE), out object dte))
        exitCode = (dte as DTE2).Solution.SolutionBuild.LastBuildInfo;

      // VS compile detected errors and there is not necessary to run clang compile
      if (0 != exitCode)
      {
        mExecuteCompile = false;
        return;
      }

      // Run clang compile after the VS compile succeeded 
      RunClangCompile();
      mExecuteCompile = false;
    }


    public void RunClangCompile()
    {
      if (mCommandsController.Running)
        return;

      mCommandsController.Running = true;

      var task = System.Threading.Tasks.Task.Run(() =>
      {
        try
        {
          if(VsServiceProvider.TryGetService(typeof(DTE), out object dte))
          {
            DocumentsHandler.SaveActiveDocuments();
            AutomationUtil.SaveDirtyProjects((dte as DTE2).Solution);
          }

          CollectSelectedItems(false, ScriptConstants.kAcceptedFileExtensions);
          RunScript(OutputWindowConstants.kComplileCommand);
        }
        catch (Exception exception)
        {
          VsShellUtilities.ShowMessageBox(AsyncPackage, exception.Message, "Error",
            OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
      }).ContinueWith(tsk => mCommandsController.OnAfterClangCommand());
    }

    #endregion

  }
}

