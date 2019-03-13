using ClangPowerTools.Output;
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
    #region Properties


    /// <summary>
    /// Gets the instance of the command.
    /// </summary>
    public static CompileCommand Instance
    {
      get;
      private set;
    }


    public bool VsCompileFlag { get; set; }


    #endregion


    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="CompileCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    private CompileCommand(OleMenuCommandService aCommandService, CommandsController aCommandsController, 
      AsyncPackage aPackage, Guid aGuid, int aId)
        : base(aPackage, aGuid, aId)
    {
      if (null != aCommandService)
      {
        var menuCommandID = new CommandID(CommandSet, Id);
        var menuCommand = new OleMenuCommand(aCommandsController.Execute, menuCommandID);
        menuCommand.BeforeQueryStatus += aCommandsController.OnBeforeClangCommand;
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
      AsyncPackage aPackage, Guid aGuid, int aId)
    {
      // Switch to the main thread - the call to AddCommand in CompileCommand's constructor requires
      // the UI thread.
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(aPackage.DisposalToken);

      OleMenuCommandService commandService = await aPackage.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
      Instance = new CompileCommand(commandService, aCommandsController, aPackage, aGuid, aId);
    }


    public System.Threading.Tasks.Task RunClangCompileAsync(int aCommandId)
    {
      return System.Threading.Tasks.Task.Run(() =>
      {
        try
        {
          if(VsServiceProvider.TryGetService(typeof(DTE), out object dte))
          {
            DocumentsHandler.SaveActiveDocuments();
            AutomationUtil.SaveDirtyProjects((dte as DTE2).Solution);
          }

          CollectSelectedItems(false, ScriptConstants.kAcceptedFileExtensions);
          RunScript(aCommandId);
        }
        catch (Exception exception)
        {
          VsShellUtilities.ShowMessageBox(AsyncPackage, exception.Message, "Error",
            OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
      });
    }

    #endregion

  }
}

