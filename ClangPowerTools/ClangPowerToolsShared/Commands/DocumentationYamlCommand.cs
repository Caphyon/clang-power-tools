using ClangPowerTools;
using ClangPowerTools.Commands;
using ClangPowerTools.Events;
using ClangPowerToolsShared.Helpers;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using Task = System.Threading.Tasks.Task;


namespace ClangPowerToolsShared.Commands
{
  public sealed class DocumentationYamlCommand : CompileCommand
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
      Instance = new DocumentationYamlCommand(commandService, aCommandController, aPackage, aGuid, aId);

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


    internal void OpenInFileExplorer(object sender, JsonFilePathArgs e)
    {
      if (!File.Exists(e.FilePath))
        return;

      // combine the arguments together
      // it doesn't matter if there is a space after ','
      string argument = "/select, \"" + e.FilePath + "\"";

      // open the file in File Explorer and select it
      Process.Start("explorer.exe", argument);

    }

  }
}
