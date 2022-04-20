using ClangPowerTools.Events;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using Task = System.Threading.Tasks.Task;

namespace ClangPowerTools.Commands
{
  /// <summary>
  /// Command handler
  /// </summary>
  internal sealed class JsonCompilationDatabaseCommand : CompileCommand
  {
    #region Properties

    /// <summary>
    /// Gets the instance of the command.
    /// </summary>
    public static new JsonCompilationDatabaseCommand Instance
    {
      get;
      private set;
    }

    #endregion


    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonCompilationDatabaseCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    /// <param name="commandService">Command service to add command to, not null.</param>
    private JsonCompilationDatabaseCommand(OleMenuCommandService aCommandService, CommandController aCommandController,
      AsyncPackage aPackage, Guid aGuid, int aId) : base(aCommandService, aCommandController, aPackage, aGuid, aId) { }

    #endregion


    #region Methods

    /// <summary>
    /// Initializes the singleton instance of the command.
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    public static new async Task InitializeAsync(CommandController aCommandController,
          AsyncPackage aPackage, Guid aGuid, int aId)
    {
      // Switch to the main thread - the call to AddCommand in ClangFormatCommand's constructor requires
      // the UI thread.
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(aPackage.DisposalToken);

      OleMenuCommandService commandService = await aPackage.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
      Instance = new JsonCompilationDatabaseCommand(commandService, aCommandController, aPackage, aGuid, aId);
    }

    private bool OpenInExplorer { get; set; }
    /// <summary>
    /// This function is the callback used to execute the command when the menu item is clicked.
    /// See the constructor to see how the menu item is associated with this function using
    /// OleMenuCommandService service and MenuCommand class.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event args.</param>
    public async Task ExportAsync(bool openInExplorer = true)
    {
      OpenInExplorer = openInExplorer;
      await RunClangCompileAsync(CommandIds.kCompileId, CommandUILocation.ContextMenu, true);
    }


    internal void OpenInFileExplorer(object sender, JsonFilePathArgs e)
    {
      if (OpenInExplorer)
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

    #endregion

    public string SolutionPath()
    {
      var dte = this.ServiceProvider.GetService(typeof(EnvDTE.DTE)) as EnvDTE80.DTE2;
      FileInfo fileInfo = new FileInfo(dte.Solution.FullName);
      return fileInfo.Directory.FullName;
    }
  }
}
