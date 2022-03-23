using ClangPowerTools;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;

namespace ClangPowerTools.Commands
{
  public sealed class DocumentationCommand : ClangCommand
  {

    private readonly AsyncPackage package;

    /// <summary>
    /// Gets the instance of the command.
    /// </summary>
    /// 
    public static DocumentationCommand Instance
    {
      get;
      private set;
    }

    private DocumentationCommand(OleMenuCommandService aCommandService, CommandController aCommandController,
  AsyncPackage aPackage, Guid aGuid, int aId)
    : base(aPackage, aGuid, aId)
    {
      if (null != aCommandService)
      {
        package = aPackage;
        var menuCommandID = new CommandID(CommandSet, Id);
        var menuCommand = new OleMenuCommand(aCommandController.Execute, menuCommandID);
        //menuCommand.BeforeQueryStatus += aCommandController.OnBeforeClangCommand;
        menuCommand.Enabled = true;
        aCommandService.AddCommand(menuCommand);
      }
    }

    /// <summary>
    /// Initializes the singleton instance of the command.
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    public static async Task InitializeAsync(CommandController aCommandController,
      AsyncPackage aPackage, Guid aGuid, int aId)
    {
      // Switch to the main thread - the call to DocumentationCommand in TidyCommand's constructor requires
      // the UI thread.
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(aPackage.DisposalToken);

      OleMenuCommandService commandService = await aPackage.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
      Instance = new DocumentationCommand(commandService, aCommandController, aPackage, aGuid, aId);

    }


  }
}
