using ClangPowerTools.Output;
using ClangPowerTools.Services;
using ClangPowerTools.SilentFile;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Linq;
using System.Windows.Threading;
using System.Windows;
using ClangPowerTools.DialogPages;
using System.Text;
using System.Collections.Generic;

namespace ClangPowerTools.Commands
{
  /// <summary>
  /// Command handler
  /// </summary>
  internal sealed class IgnoreCompileCommand : BasicCommand
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
    private IgnoreCompileCommand(CommandsController aCommandsController, OleMenuCommandService aCommandService, AsyncPackage aPackage, Guid aGuid, int aId)
      : base(aPackage, aGuid, aId)
    {
      if (null != aCommandService)
      {
        var menuCommandID = new CommandID(CommandSet, Id);
        var menuItem = new OleMenuCommand(aCommandsController.Execute, menuCommandID);
        aCommandService.AddCommand(menuItem);
      }
    }

    #endregion


    #region Public Methods

    /// <summary>
    /// Initializes the singleton instance of the command.
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    public static async System.Threading.Tasks.Task InitializeAsync(CommandsController aCommandsController,
      AsyncPackage aPackage, Guid aGuid, int aId)
    {
      // Switch to the main thread - the call to AddCommand in IgnoreCompileCommand's constructor requires
      // the UI thread.
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(aPackage.DisposalToken);

      OleMenuCommandService commandService = await aPackage.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
      Instance = new IgnoreCompileCommand(aCommandsController, commandService, aPackage, aGuid, aId);
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
      var task = System.Threading.Tasks.Task.Run(() =>
      {
        List<string> documentsToIgnore = DocumentsHandler.GetDocumentsToIgnore();
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
    private void AddIgnoreFilesToSettings(List<string> documentsToIgnore)
    {
      var settings = SettingsProvider.GeneralSettings;

      if (settings.FilesToIgnore.Length > 0)
      {
        settings.FilesToIgnore += ";";
      }
      settings.FilesToIgnore += string.Join(";", RemoveDuplicateFiles(documentsToIgnore, settings));
      settings.SaveSettingsToStorage();
    }

    private List<string> RemoveDuplicateFiles(List<string> documentsToIgnore, ClangGeneralOptionsView settings)
    {
      List<string> trimmedDocumentToIgnore = new List<string>();

      foreach (var item in documentsToIgnore)
      {
        if (!settings.FilesToIgnore.Contains(item))
        {
          trimmedDocumentToIgnore.Add(item);
        }
      }
      return trimmedDocumentToIgnore;
    }
    #endregion
  }
}
