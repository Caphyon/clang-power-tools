using ClangPowerTools;
using ClangPowerTools.Commands;
using ClangPowerTools.Events;
using ClangPowerToolsShared.Helpers;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using Task = System.Threading.Tasks.Task;


namespace ClangPowerToolsShared.Commands
{
  public sealed class DocumentationMdCommand : CompileCommand
  {
    private AsyncPackage package;

    public static DocumentationMdCommand Instance
    {
      get;
      private set;
    }

    private DocumentationMdCommand(OleMenuCommandService aCommandService, CommandController aCommandController,
      AsyncPackage aPackage, Guid aGuid, int aId) : base(aCommandService, aCommandController, aPackage, aGuid, aId) { package = aPackage; }

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

    public async Task GenerateDocumentationAsync(bool jsonCompilationDbActive, int commmandId)
    {
      await GenerateDocumentation.GenerateDocumentationForProjectAsync(commmandId, jsonCompilationDbActive, package);

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
