using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using Task = System.Threading.Tasks.Task;

namespace ClangPowerTools.Commands
{
  /// <summary>
  /// Command handler
  /// </summary>
  internal sealed class TidyConfigCommand : BasicCommand
  {
    #region Properties
    /// <summary>
    /// Gets the instance of the command.
    /// </summary>
    public static TidyConfigCommand Instance
    {
      get;
      private set;
    }
    #endregion

    #region Member Variables
    SaveFileDialog saveFileDialog = new SaveFileDialog();
    #endregion

    #region Constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="TidyConfigCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    /// <param name="commandService">Command service to add command to, not null.</param>
    private TidyConfigCommand(CommandsController aCommandsController, OleMenuCommandService aCommandService, AsyncPackage aPackage, Guid aGuid, int aId)
      : base(aPackage, aGuid, aId)
    {
      if (null != aCommandService)
      {
        var menuCommandID = new CommandID(CommandSet, Id);
        var menuItem = new OleMenuCommand(aCommandsController.ExecuteAsync, menuCommandID);
        aCommandService.AddCommand(menuItem);
      }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Initializes the singleton instance of the command.
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    public static async Task InitializeAsync(CommandsController aCommandsController, AsyncPackage aPackage, Guid aGuid, int aId)
    {
      // Switch to the main thread - the call to AddCommand in SettingsCommand's constructor requires
      // the UI thread.
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(aPackage.DisposalToken);

      OleMenuCommandService commandService = await aPackage.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
      Instance = new TidyConfigCommand(aCommandsController, commandService, aPackage, aGuid, aId);
    }

    /// <summary>
    /// This function is the callback used to execute the command when the menu item is clicked.
    /// See the constructor to see how the menu item is associated with this function using
    /// OleMenuCommandService service and MenuCommand class.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event args.</param>
    public void ExportConfig()
    {
      // Set the default file extension
      saveFileDialog.DefaultExt = ".clang-tidy";
      saveFileDialog.Filter = "Configuration files (.clang-tidy)|*.clang-tidy";

      //Display the dialog window
      bool? result = saveFileDialog.ShowDialog();

      if (result == true)
      {
        saveFileDialog.FileOk += SaveFileDialog;
      }
    }
    #endregion

    #region Private Methods
    private void SaveFileDialog(object sender, CancelEventArgs e)
    {
      CreateFile();
    }


    private void CreateFile()
    {
      using (FileStream fs = new FileStream(saveFileDialog.FileName, FileMode.Create))
      {
        using (StreamWriter sw = new StreamWriter(fs))
        {
          sw.Write("Hello world!");
        }
      }
    }

    #endregion
  }
}
