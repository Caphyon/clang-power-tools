using ClangPowerTools;
using ClangPowerTools.Services;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Task = System.Threading.Tasks.Task;

namespace ClangPowerTools.Commands
{
  public sealed class DocumentationCommand : CompileCommand
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
      Instance = new DocumentationCommand(commandService, aCommandController, aPackage, aGuid, aId);

    }

    public async Task GenerateDocumentationAsync(bool jsonCompilationDbActive)
    {
      //generate compilation database
      await RunClangCompileAsync(CommandIds.kCompileId, CommandUILocation.ContextMenu, true);
      //await PrepareCommmandAsync(CommandUILocation.ContextMenu, jsonCompilationDbActive);
      //CacheProjectsFromItems();

      FilePathCollector fileCollector = new FilePathCollector();
      var paths = fileCollector.Collect(mItemsCollector.Items).ToList();
      //CommandControllerInstance.CommandController.LaunchCommandAsync(CommandIds.kJsonCompilationDatabase, CommandUILocation.ContextMenu);

      string projectPath = string.Empty;
      if(paths.Any())
      {
        FileInfo fileInfo = new FileInfo(paths.FirstOrDefault());
        projectPath = fileInfo.Directory.Parent.FullName;
      }

      string jsonCompilationDatabasePath = Path.Combine(projectPath, ScriptConstants.kCompilationDBFile);

      var formatSettings = SettingsProvider.FormatSettingsModel;
      var getllvm = GetScriptFilePath();
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


    protected string GetScriptFilePath()
    {
      var assemblyPath = Assembly.GetExecutingAssembly().Location;
      var scriptDirectory = assemblyPath.Substring(0, assemblyPath.LastIndexOf('\\'));

      return Path.Combine(scriptDirectory, "Tooling\\v1\\psClang", ScriptConstants.kGetLLVMScriptName);
    }

  }
}
