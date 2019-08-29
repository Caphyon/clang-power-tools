using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.Collections.Generic;
using Task = System.Threading.Tasks.Task;

namespace ClangPowerTools.Commands
{
  /// <summary>
  /// Command handler
  /// </summary>
  public sealed class IgnoreCompileCommand : BasicCommand
  {
    #region Properties

    /// <summary>
    /// Gets the instance of the command.
    /// </summary>
    public static IgnoreCompileCommand Instance
    {
      get;
      private set;
    }
    #endregion


    #region Constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="IgnoreCompileCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    /// <param name="commandService">Command service to add command to, not null.</param>
    private IgnoreCompileCommand(CommandController aCommandController, OleMenuCommandService aCommandService, AsyncPackage aPackage, Guid aGuid, int aId)
      : base(aPackage, aGuid, aId)
    {
      if (null != aCommandService)
      {
        var menuCommandID = new CommandID(CommandSet, Id);
        var menuItem = new OleMenuCommand(aCommandController.Execute, menuCommandID);
        aCommandService.AddCommand(menuItem);
      }
    }

    #endregion


    #region Public Methods

    /// <summary>
    /// Initializes the singleton instance of the command.
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    public static async Task InitializeAsync(CommandController aCommandController,
      AsyncPackage aPackage, Guid aGuid, int aId)
    {
      // Switch to the main thread - the call to AddCommand in IgnoreCompileCommand's constructor requires
      // the UI thread.
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(aPackage.DisposalToken);

      OleMenuCommandService commandService = await aPackage.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
      Instance = new IgnoreCompileCommand(aCommandController, commandService, aPackage, aGuid, aId);
    }

    /// <summary>
    /// This function is the callback used to execute the command when the menu item is clicked.
    /// See the constructor to see how the menu item is associated with this function using
    /// OleMenuCommandService service and MenuCommand class.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event args.</param>
    public void RunIgnoreCompileCommand(int aId)
    {
      var task = Task.Run(() =>
      {
        List<string> documentsToIgnore = ItemsCollector.GetDocumentsToIgnore();
        AddIgnoreFilesToSettings(documentsToIgnore);
      });
    }

    #endregion


    #region Private Methods
    /// <summary>
    /// Create ignore format string for the settings page
    /// </summary>
    /// <param name="documentsToIgnore"></param>
    /// <returns></returns>
    public void AddIgnoreFilesToSettings(List<string> documentsToIgnore)
    {
      string filesToIgnore = SettingsViewModelProvider.CompilerSettingsViewModel.CompilerModel.FilesToIgnore;

      if (filesToIgnore.Length > 0)
      {
        filesToIgnore += ";";
      }
      filesToIgnore += string.Join(";", RemoveDuplicateFiles(documentsToIgnore));

      SettingsViewModelProvider.CompilerSettingsViewModel.CompilerModel.FilesToIgnore = filesToIgnore;
    }

    private List<string> RemoveDuplicateFiles(List<string> documentsToIgnore)
    {
      List<string> trimmedDocumentToIgnore = new List<string>();
      string filesToIgnore = SettingsViewModelProvider.CompilerSettingsViewModel.CompilerModel.FilesToIgnore;

      foreach (var item in documentsToIgnore)
      {
        if (!filesToIgnore.Contains(item))
        {
          trimmedDocumentToIgnore.Add(item);
        }
      }
      return trimmedDocumentToIgnore;
    }
    #endregion
  }
}
