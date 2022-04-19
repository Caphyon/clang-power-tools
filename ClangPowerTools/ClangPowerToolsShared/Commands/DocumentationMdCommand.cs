using ClangPowerTools;
using ClangPowerTools.Commands;
using ClangPowerTools.Events;
using ClangPowerToolsShared.Helpers;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using Task = System.Threading.Tasks.Task;


namespace ClangPowerToolsShared.Commands
{
  public sealed class DocumentationMdCommand : ClangCommand
  {
    private AsyncPackage package;

    public static DocumentationMdCommand Instance
    {
      get;
      private set;
    }

    private DocumentationMdCommand(OleMenuCommandService aCommandService, CommandController aCommandController,
      AsyncPackage aPackage, Guid aGuid, int aId)
        : base(aPackage, aGuid, aId)
    {
      package = aPackage;
      if (null != aCommandService)
      {
        var menuCommandID = new CommandID(CommandSet, Id);
        var menuCommand = new OleMenuCommand(aCommandController.Execute, menuCommandID);
        menuCommand.BeforeQueryStatus += aCommandController.OnBeforeClangCommand;
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
      Instance = new DocumentationMdCommand(commandService, aCommandController, aPackage, aGuid, aId);
    }

    public async Task GenerateDocumentationAsync(int commandId)
    {
      await Task.Run(() =>
      {
        lock (mutex)
        {
          try
          {
            GenerateDocumentationForProjectAsync(commandId, package);
          }
          catch (Exception exception)
          {
            VsShellUtilities.ShowMessageBox(AsyncPackage, exception.Message, "Error",
              OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
          }
        }
      });
    }
  }
}
