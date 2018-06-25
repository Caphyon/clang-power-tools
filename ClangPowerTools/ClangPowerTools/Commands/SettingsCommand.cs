﻿using System;
using System.ComponentModel.Design;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace ClangPowerTools
{
  /// <summary>
  /// Command handler
  /// </summary>
  internal sealed class SettingsCommand : BasicCommand
  {
    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    public SettingsCommand(DTE2 aDte, AsyncPackage aPackage, Guid aGuid, int aId)
      : base(aDte, aPackage, aGuid, aId)
    {
      var commandService = ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;

      if (null != commandService)
      {
        var menuCommandID = new CommandID(CommandSet, Id);
        var menuItem = new MenuCommand(this.ShowSettings, menuCommandID);
        commandService.AddCommand(menuItem);
      }
    }

    #endregion


    #region Private Methods

    /// <summary>
    /// This function is the callback used to execute the command when the menu item is clicked.
    /// See the constructor to see how the menu item is associated with this function using
    /// OleMenuCommandService service and MenuCommand class.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event args.</param>
    private void ShowSettings(object sender, EventArgs e)
    {
      AsyncPackage.ShowOptionPage(typeof(ClangGeneralOptionsView));
    }

    #endregion

  }
}
