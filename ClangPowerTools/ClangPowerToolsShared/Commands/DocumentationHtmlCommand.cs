using ClangPowerTools;
using ClangPowerTools.Commands;
using ClangPowerTools.Events;
using ClangPowerToolsShared.Helpers;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using Task = System.Threading.Tasks.Task;

namespace ClangPowerToolsShared.Commands
{
  public sealed class DocumentationHtmlCommand : CompileCommand
  {
    public event EventHandler<CloseDataStreamingEventArgs> CloseDataStreamingEvent;
    protected void OnDataStreamClose(CloseDataStreamingEventArgs e)
    {
      CloseDataStreamingEvent?.Invoke(this, e);
    }

    public static DocumentationHtmlCommand Instance
    {
      get;
      private set;
    }

    private DocumentationHtmlCommand(OleMenuCommandService aCommandService, CommandController aCommandController,
      AsyncPackage aPackage, Guid aGuid, int aId) : base(aCommandService, aCommandController, aPackage, aGuid, aId) { }

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
      Instance = new DocumentationHtmlCommand(commandService, aCommandController, aPackage, aGuid, aId);
    }

    public async Task GenerateDocumentationAsync(bool jsonCompilationDbActive, int commandId)
    {
      await PrepareCommmandAsync(CommandUILocation.ContextMenu, jsonCompilationDbActive);
      CacheProjectsFromItems();

      FilePathCollector fileCollector = new FilePathCollector();
      var paths = fileCollector.Collect(mItemsCollector.Items).ToList();
      GenerateDocumentation.GenerateDocumentationForProject(commandId, jsonCompilationDbActive, paths);
      
      if (StopCommandActivated)
      {
        OnDataStreamClose(new CloseDataStreamingEventArgs(true));
        StopCommandActivated = false;
      }
      else
      {
        OnDataStreamClose(new CloseDataStreamingEventArgs(false));
      }
    }

  }
}
