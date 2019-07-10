using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace ClangPowerTools.Commands
{
  /// <summary>
  /// Command handler
  /// </summary>
  internal sealed class EncodingConverter : BasicCommand
  {
    #region Properties

    /// <summary>
    /// Gets the instance of the command.
    /// </summary>
    public static EncodingConverter Instance
    {
      get;
      private set;
    }
    #endregion

    #region Constructor
    private EncodingConverter(CommandController commandController, AsyncPackage package, OleMenuCommandService commandService, Guid guid, int id)
         : base(package, guid, id)
    {
      if (commandService != null)
      {
        var menuCommandID = new CommandID(CommandSet, Id);
        var menuItem = new OleMenuCommand(commandController.Execute, menuCommandID);
        commandService.AddCommand(menuItem);
      }
    }
    #endregion

    #region Public Methods

    /// <summary>
    /// Initializes the singleton instance of the command.
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    public static async Task InitializeAsync(CommandController commandController, AsyncPackage package, Guid guid, int id)
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

      OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
      Instance = new EncodingConverter(commandController, package, commandService, guid, id);
    }


    public void RunEncodingConverter(int id)
    {

    }

    /// <summary>
    /// This function is the callback used to execute the command when the menu item is clicked.
    /// See the constructor to see how the menu item is associated with this function using
    /// OleMenuCommandService service and MenuCommand class.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event args.</param>
    //private void Execute(object sender, EventArgs e)
    //{

    //}
    #endregion
  }
}
