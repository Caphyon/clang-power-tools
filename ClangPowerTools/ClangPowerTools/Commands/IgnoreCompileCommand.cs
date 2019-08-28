﻿using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using Task = System.Threading.Tasks.Task;

namespace ClangPowerTools.Commands
{
  /// <summary>
  /// Command handler
  /// </summary>
  public sealed class IgnoreCompileCommand : IgnoreCommand<ClangGeneralOptionsView>
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
        List<string> projectsToIgnore = ItemsCollector.GetProjectsToIgnore();
        var settings = SettingsProvider.GeneralSettings;
        AddIgnoreItemsToSettings(projectsToIgnore, settings, "ProjectsToIgnore");
        if (projectsToIgnore.Any() == false)
        {
          List<string> filesToIgnore = ItemsCollector.GetFilesToIgnore();
          AddIgnoreItemsToSettings(filesToIgnore, settings, "FilesToIgnore");
          settings.SaveSettingsToStorage();
        }
      });
    }

    #endregion
  }
}
