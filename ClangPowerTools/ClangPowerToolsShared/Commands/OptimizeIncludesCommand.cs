using Microsoft.VisualStudio.Shell;
using System.ComponentModel.Design;
using System;
using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio.Shell.Interop;
using System.Windows.Forms;
using System.IO;
using ClangPowerToolsShared.MVVM.Constants;

namespace ClangPowerTools.Commands
{
  public class OptimizeIncludesCommand : ClangCommand
  {
    /// <summary>
    /// Gets the instance of the command.
    /// </summary>
    public static OptimizeIncludesCommand Instance
    {
      get;
      private set;
    }

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="CompileCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    protected OptimizeIncludesCommand(OleMenuCommandService aCommandService, CommandController aCommandController,
      AsyncPackage aPackage, Guid aGuid, int aId)
        : base(aPackage, aGuid, aId)
    {
      if (null != aCommandService)
      {
        var menuCommandID = new CommandID(CommandSet, Id);
        var menuCommand = new OleMenuCommand(aCommandController.Execute, menuCommandID);
        menuCommand.BeforeQueryStatus += aCommandController.OnBeforeClangCommand;
        menuCommand.Enabled = true;
        aCommandService.AddCommand(menuCommand);
      }
    }

    #endregion

    /// <summary>
    /// Initializes the singleton instance of the command.
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    public static async Task InitializeAsync(CommandController aCommandController,
      AsyncPackage aPackage, Guid aGuid, int aId)
    {
      // Switch to the main thread - the call to AddCommand in CompileCommand's constructor requires
      // the UI thread.
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(aPackage.DisposalToken);

      OleMenuCommandService commandService = await aPackage.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
      Instance = new OptimizeIncludesCommand(commandService, aCommandController, aPackage, aGuid, aId);
    }

    public async Task RunOptimizeIncludes(CommandUILocation commandUILocation, bool jsonCompilationDbActive = false)
    {
      //generate compilation database
      await CommandControllerInstance.CommandController.LaunchCommandAsync(aCommandId: CommandIds.kJsonCompilationDatabase,
        aCommandUILocation: commandUILocation, openCompilationDatabaseInExplorer: false);

      //downlaod tools
      //include what you use
      var jsonCompilationDatabasePath = PathConstants.JsonCompilationDBPath;
      string iwyuFilePath = Path.Combine(new FileInfo(jsonCompilationDatabasePath).Directory.FullName,
        "iwyu.txt");
      string iwyuExe = PowerShellWrapper.DownloadTool(ScriptConstants.kIwyu);
      string iwyuTool = PowerShellWrapper.DownloadTool(ScriptConstants.kIwyuTool);


      //apply include what you use


    }
  }
}
