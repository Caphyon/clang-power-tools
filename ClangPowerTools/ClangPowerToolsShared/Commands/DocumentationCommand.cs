using ClangPowerTools;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
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

    public async Task GenerateDocumentationAsync()
    {
      await PrepareCommmandAsync(CommandUILocation.ContextMenu);
      var formatSettings = SettingsProvider.FormatSettingsModel;

      string vsixPath = Path.GetDirectoryName(
        GetType().Assembly.Location);
      //TODO Verify if compilation database exists
      Process process = new Process();
      process.StartInfo.UseShellExecute = false;
      process.StartInfo.CreateNoWindow = true;
      process.StartInfo.RedirectStandardInput = true;
      process.StartInfo.RedirectStandardOutput = true;
      process.StartInfo.RedirectStandardError = true;
      process.StartInfo.FileName =
              true == (string.IsNullOrWhiteSpace(formatSettings.CustomExecutable) == false) ?
              formatSettings.CustomExecutable : Path.Combine(vsixPath, ScriptConstants.kClangDoc);
      process.StartInfo.Arguments = $" {ScriptConstants.kCompilationDBFile}";

      try
      {
        process.Start();
      }
      catch (Exception exception)
      {
        throw new Exception(
            $"Cannot execute {process.StartInfo.FileName}.\n{exception.Message}.");
      }
    }


  }
}
