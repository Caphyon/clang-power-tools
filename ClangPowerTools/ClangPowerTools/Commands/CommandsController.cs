using ClangPowerTools.Commands;
using ClangPowerTools.Handlers;
using ClangPowerTools.Output;
using ClangPowerTools.Services;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;

namespace ClangPowerTools
{
  /// <summary>
  /// Contains all the logic of disable and enable the clang custom commands  
  /// </summary>
  public class CommandsController
  {
    #region Members

    public static readonly Guid mCommandSet = new Guid("498fdff5-5217-4da9-88d2-edad44ba3874");


    #region Commands



    #endregion


    #endregion


    #region Properties


    /// <summary>
    /// Running flag for clang commands
    /// </summary>
    public bool Running { get; set; }


    /// <summary>
    /// Running flag for Visual Studio build
    /// </summary>
    public bool VsBuildRunning { get; set; }


    #endregion


    #region Public Methods


    public void Execute(object sender, EventArgs e)
    {
      if (!(sender is OleMenuCommand command))
        return;

      switch (command.CommandID.ID)
      {
        case CommandIds.kSettingsId:
          SettingsCommand.Instance.ShowSettings();
            break;

        case CommandIds.kStopClang:
          StopClang.Instance.RunStopClangCommand();
          break;

        case CommandIds.kClangFormat:
          ClangFormatCommand.Instance.RunClangFormat();
          break;

        case CommandIds.kCompileId:
          CompileCommand.Instance.RunClangCompile();
          break;

        case CommandIds.kTidyId:
          TidyCommand.Instance.RunClangTidy(false);
          break;

        case CommandIds.kTidyFixId:
          TidyCommand.Instance.RunClangTidy(true);
          break;
      }
    }


    public async System.Threading.Tasks.Task InitializeAsyncCommands(AsyncPackage aAsyncPackage, 
      ErrorWindowController aErrorController, OutputWindowController aOutputWindowController)
    {
      if (null == CompileCommand.Instance)
        await CompileCommand.InitializeAsync(this, aErrorController, aOutputWindowController, aAsyncPackage, mCommandSet, CommandIds.kCompileId);

      if (null == TidyCommand.Instance)
      {
        await TidyCommand.InitializeAsync(this, aErrorController, aOutputWindowController, aAsyncPackage, mCommandSet, CommandIds.kTidyId);
        await TidyCommand.InitializeAsync(this, aErrorController, aOutputWindowController, aAsyncPackage, mCommandSet, CommandIds.kTidyFixId);
      }

      if (null == ClangFormatCommand.Instance)
        await ClangFormatCommand.InitializeAsync(this, aErrorController, aOutputWindowController, aAsyncPackage, mCommandSet, CommandIds.kClangFormat);

      if (null == StopClang.Instance)
        await StopClang.InitializeAsync(this, aErrorController, aOutputWindowController, aAsyncPackage, mCommandSet, CommandIds.kStopClang);

      if (null == SettingsCommand.Instance)
        await SettingsCommand.InitializeAsync(this, aAsyncPackage, mCommandSet, CommandIds.kSettingsId);
    }




    /// <summary>
    /// It is called immediately after every clang command execution.
    /// Set the running state to false.
    /// </summary>
    public void OnAfterClangCommand()
    {
      UIUpdater.Invoke(() =>
      {
        Running = false;
      });
    }


    /// <summary>
    /// It is called before every command. Update the running state.  
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void OnBeforeClangCommand(object sender, EventArgs e)
    {
      UIUpdater.Invoke(() =>
      {
        if (!(sender is OleMenuCommand command))
          return;

        if (VsServiceProvider.TryGetService(typeof(DTE), out object dte) && !(dte as DTE2).Solution.IsOpen)
          command.Visible = command.Enabled = false;

        else if (VsBuildRunning && command.CommandID.ID != CommandIds.kSettingsId)
          command.Visible = command.Enabled = false;

        else
          command.Visible = command.Enabled = command.CommandID.ID != CommandIds.kStopClang ? !Running : Running;
      });
    }

    /// <summary>
    /// Set the VS running build flag to true when the VS build begin.
    /// </summary>
    /// <param name="Scope"></param>
    /// <param name="Action"></param>
    public void OnBuildBegin(vsBuildScope Scope, vsBuildAction Action) => VsBuildRunning = true;

    /// <summary>
    /// Set the VS running build flag to false when the VS build finished.
    /// </summary>
    /// <param name="Scope"></param>
    /// <param name="Action"></param>
    public void OnBuildDone(vsBuildScope Scope, vsBuildAction Action) => VsBuildRunning = false;


    #endregion


    private bool SetTidyFixParameter(object sender)
    {
      if (!(sender is OleMenuCommand))
        return false;

      return (sender as OleMenuCommand).CommandID.ID == CommandIds.kTidyFixId;
    }

  }
}
