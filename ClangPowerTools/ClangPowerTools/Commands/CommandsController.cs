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


    #region Events


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
    public void OnBuildDone(vsBuildScope Scope, vsBuildAction Action)
    {
      VsBuildRunning = false;
      OnBuildDoneClangCompile();


    }

    #endregion


    #region Commands Events


    public void CommandEventsBeforeExecute(string aGuid, int aId, object aCustomIn, object aCustomOut, ref bool aCancelDefault)
    {
      BeforeExecuteClangCompile(aGuid, aId);
    }

    private void BeforeExecuteClangCompile(string aGuid, int aId)
    {
      if (false == mGeneralOptions.ClangCompileAfterVsCompile)
        return;

      string commandName = GetCommandName(aGuid, aId);
      if (0 != string.Compare("Build.Compile", commandName))
        return;

      CompileCommand.Instance.VsCompileFlag = true;
    }


    private void OnBuildDoneClangCompile()
    {
      if (false == CompileCommand.Instance.VsCompileFlag)
        return;

      var exitCode = int.MaxValue;
      if (VsServiceProvider.TryGetService(typeof(DTE), out object dte))
        exitCode = (dte as DTE2).Solution.SolutionBuild.LastBuildInfo;

      // VS compile detected errors and there is not necessary to run clang compile
      if (0 != exitCode)
      {
        CompileCommand.Instance.VsCompileFlag = false;
        return;
      }

      // Run clang compile after the VS compile succeeded 
      CompileCommand.Instance.RunClangCompile();
      CompileCommand.Instance.VsCompileFlag = false;
    }


    #endregion


    protected string GetCommandName(string aGuid, int aId)
    {
      try
      {
        if (null == aGuid)
          return string.Empty;

        if (null == mCommand)
          return string.Empty;

        Command cmd = mCommand.Item(aGuid, aId);
        if (null == cmd)
          return string.Empty;

        return cmd.Name;
      }
      catch (Exception) { }

      return string.Empty;
    }


    #endregion

  }
}
