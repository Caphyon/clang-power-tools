using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE;
using System.IO;

namespace ClangPowerTools
{
  /// <summary>
  /// Command handler
  /// </summary>
  internal sealed class TidyCommand : ClangCommand
  {
    #region Members

    private TidyOptions mTidyOptions;
    private TidyChecks mTidyChecks;
    private FileChangerWatcher mFileWatcher;
    private FileOpener mFileOpener;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="TidyCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>

    public TidyCommand(Package aPackage, Guid aGuid, int aId, DTE2 aDte,
      string aEdition, string aVersion, CommandsController aCommandsController)
        : base(aPackage, aGuid, aId, aDte, aEdition, aVersion, aCommandsController)
    {
      mTidyOptions = (TidyOptions)Package.GetDialogPage(typeof(TidyOptions));
      mTidyChecks = (TidyChecks)Package.GetDialogPage(typeof(TidyChecks));
      mFileOpener = new FileOpener(mDte);
      if (this.ServiceProvider.GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
      {
        var menuCommandID = new CommandID(CommandSet, Id);
        var menuCommand = new OleMenuCommand(this.MenuItemCallback, menuCommandID);
        menuCommand.BeforeQueryStatus += mCommandsController.QueryCommandHandler;
        menuCommand.Enabled = true;
        commandService.AddCommand(menuCommand);
      }
    }

    #endregion

    #region Methods


    /// <summary>
    /// This function is the callback used to execute the command when the menu item is clicked.
    /// See the constructor to see how the menu item is associated with this function using
    /// OleMenuCommandService service and MenuCommand class.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event args.</param>
    private void MenuItemCallback(object sender, EventArgs e)
    {
      mCommandsController.Running = true;
			var task = System.Threading.Tasks.Task.Run(() =>
      {
        try
        {
          SaveActiveDocuments();
          CollectSelectedItems();
          mFileWatcher = new FileChangerWatcher();
          using (var guard = new SilentFileChangerGuard())
          {
            if (mTidyOptions.Fix)
            {
              WatchFiles();
              SilentFiles(guard);
            }
            RunScript(OutputWindowConstants.kTidyCodeCommand, mTidyOptions, mTidyChecks);
          }
        }
        catch (Exception exception)
        {
          VsShellUtilities.ShowMessageBox(Package, exception.Message, "Error",
            OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
      }).ContinueWith(tsk => mCommandsController.AfterExecute()); ;
    }

    #endregion

    #region Helpers

    private void SilentFiles(SilentFileChangerGuard aGuard)
    {
      FilePathCollector fileCollector = new FilePathCollector();
      fileCollector.Collect(mItemsCollector.GetItems);

      // silent all open files
      foreach (Document doc in mDte.Documents)
        aGuard.Add(new SilentFileChanger(Package, Path.Combine(doc.Path, doc.Name), true));
      //silent all selected files
      aGuard.AddRange(Package, fileCollector.Files);
    }

    private void WatchFiles()
    {
      mFileWatcher.OnChanged += mFileOpener.FileChanged;
      string solutionFolderPath = mDte.Solution.FullName
        .Substring(0, mDte.Solution.FullName.LastIndexOf('\\'));
      mFileWatcher.Run(solutionFolderPath);
    }

    #endregion

  }
}
