using ClangPowerTools;
using ClangPowerTools.Commands;
using ClangPowerTools.Events;
using ClangPowerToolsShared.Helpers;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using Task = System.Threading.Tasks.Task;


namespace ClangPowerToolsShared.Commands
{
  public sealed class DocumentationYamlCommand : ClangCommand
  {
    //public event EventHandler<CloseDataStreamingEventArgs> CloseDataStreamingEvent;
    //protected void OnDataStreamClose(CloseDataStreamingEventArgs e)
    //{
    //  CloseDataStreamingEvent?.Invoke(this, e);
    //}
    private AsyncPackage package;

    public static DocumentationYamlCommand Instance
    {
      get;
      private set;
    }

    private DocumentationYamlCommand(OleMenuCommandService aCommandService, CommandController aCommandController,
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
      Instance = new DocumentationYamlCommand(commandService, aCommandController, aPackage, aGuid, aId);

    }

    public async Task GenerateDocumentationAsync(int commandId)
    {
      await PrepareCommmandAsync(CommandUILocation.ContextMenu, false);
      CacheProjectsFromItems();
      await Task.Run(() =>
      {
        lock (mutex)
        {
          try
          {
            GenerateDocumentationForProject(commandId, package);
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
