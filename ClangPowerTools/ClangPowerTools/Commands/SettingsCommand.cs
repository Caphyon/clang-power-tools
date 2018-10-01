using System;
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
    #region Properties


    /// <summary>
    /// Gets the instance of the command.
    /// </summary>
    public static SettingsCommand Instance
    {
      get;
      private set;
    }


    #endregion


    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    private SettingsCommand(OleMenuCommandService aCommandService, DTE2 aDte, AsyncPackage aPackage, Guid aGuid, int aId)
      : base(aDte, aPackage, aGuid, aId)
    {
      if (null != aCommandService)
      {
        var menuCommandID = new CommandID(CommandSet, Id);
        var menuItem = new MenuCommand(this.ShowSettings, menuCommandID);
        aCommandService.AddCommand(menuItem);
      }
    }

    #endregion


    #region Public Methods

    
    /// <summary>
    /// Initializes the singleton instance of the command.
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    public static async System.Threading.Tasks.Task InitializeAsync(DTE2 aDte, AsyncPackage aPackage, Guid aGuid, int aId)
    {
      // Switch to the main thread - the call to AddCommand in SettingsCommand's constructor requires
      // the UI thread.
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(aPackage.DisposalToken);

      OleMenuCommandService commandService = await aPackage.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
      Instance = new SettingsCommand(commandService, aDte, aPackage, aGuid, aId);
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
