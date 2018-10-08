using ClangPowerTools.Handlers;
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

    /// <summary>
    /// Async service provider instance
    /// </summary>
    private IAsyncServiceProvider mServiceProvider;


    #endregion


    #region Constructor

    /// <summary>
    /// Instance constructor
    /// </summary>
    /// <param name="aServiceProvider">The async service provider instance</param>
    /// <param name="aDte">DTE2 instance</param>
    public CommandsController(IAsyncServiceProvider aServiceProvider) => mServiceProvider = aServiceProvider;


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

  }
}
