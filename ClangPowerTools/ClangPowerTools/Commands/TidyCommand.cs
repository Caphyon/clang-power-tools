using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using ClangPowerTools.DialogPages;
using ClangPowerTools.SilentFile;
using System.Linq;
using EnvDTE;

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
    private TidyCustomChecks mTidyCustomChecks;
    private ClangFormatPage mClangFormat;

    private FileChangerWatcher mFileWatcher;
    private FileOpener mFileOpener;

    private bool mForceTidyToFix = false;
    private bool mSaveCommandWasGiven = false;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="TidyCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>

    public TidyCommand(Package aPackage, Guid aGuid, int aId, CommandsController aCommandsController) 
      : base(aCommandsController, aPackage, aGuid, aId)
    {
      mTidyOptions = (TidyOptions)Package.GetDialogPage(typeof(TidyOptions));
      mTidyChecks = (TidyChecks)Package.GetDialogPage(typeof(TidyChecks));
      mTidyCustomChecks = (TidyCustomChecks)Package.GetDialogPage(typeof(TidyCustomChecks));

      mFileOpener = new FileOpener(DTEObj);

      if (ServiceProvider.GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
      {
        var menuCommandID = new CommandID(CommandSet, Id);
        var menuCommand = new OleMenuCommand(this.RunClangTidy, menuCommandID);
        menuCommand.BeforeQueryStatus += mCommandsController.QueryCommandHandler;
        menuCommand.Enabled = true;
        commandService.AddCommand(menuCommand);
      }
    }

    #endregion

    #region Public Methods

    public override void OnBeforeSave(object sender, Document aDocument)
    {
      if (false == mSaveCommandWasGiven) // The save event was not triggered by Save File or SaveAll commands
        return;

      if (false == mTidyOptions.AutoTidyOnSave) // The clang-tidy on save option is disable 
        return;

      if (true == mCommandsController.Running) // Clang compile/tidy command is running
        return;

      if (true == mForceTidyToFix) // Clang-tidy on save is running 
        return;

      mForceTidyToFix = true;
      RunClangTidy(new object(), new EventArgs());
      mSaveCommandWasGiven = false;
    }

    public override void CommandEventsBeforeExecute(string aGuid, int aId, object aCustomIn, object aCustomOut, ref bool aCancelDefault)
    {
      string commandName = GetCommandName(aGuid, aId);
      if (0 != string.Compare("File.SaveAll", commandName) &&
        0 != string.Compare("File.SaveSelectedItems", commandName))
      {
        return;
      }
      mSaveCommandWasGiven = true;
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
    private void RunClangTidy(object sender, EventArgs e)
    {
      mCommandsController.Running = true;
      var task = System.Threading.Tasks.Task.Run(() =>
      {
        try
        {
          DocumentsHandler.SaveActiveDocuments((DTE)DTEObj);
          AutomationUtil.SaveDirtyProjects(ServiceProvider, DTEObj.Solution);

          CollectSelectedItems(ScriptConstants.kAcceptedFileExtensions);

          mFileWatcher = new FileChangerWatcher();
          mFileOpener = new FileOpener(DTEObj);
          var silentFileController = new SilentFileController();

          using (var guard = silentFileController.GetSilentFileChangerGuard())
          {
            if (true == mTidyOptions.Fix || true == mTidyOptions.AutoTidyOnSave)
            {
              WatchFiles();

              FilePathCollector fileCollector = new FilePathCollector();
              var filesPath = fileCollector.Collect(mItemsCollector.GetItems).ToList();

              silentFileController.SilentFiles(Package, guard, filesPath);
              silentFileController.SilentOpenFiles(Package, guard, DTEObj);
            }
            RunScript(OutputWindowConstants.kTidyCodeCommand, mTidyOptions, mTidyChecks, mTidyCustomChecks, mClangFormat);
          }
        }
        catch (Exception exception)
        {
          VsShellUtilities.ShowMessageBox(Package, exception.Message, "Error",
            OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
        finally
        {
          mForceTidyToFix = false;
        }
      }).ContinueWith(tsk => mCommandsController.AfterExecute());
    }

    #endregion

    #region Helpers

    private void WatchFiles()
    {
      try
      {
        mFileWatcher.OnChanged += mFileOpener.FileChanged;
        string solutionFolderPath = DTEObj.Solution.FullName
          .Substring(0, DTEObj.Solution.FullName.LastIndexOf('\\'));
        mFileWatcher.Run(solutionFolderPath);
      }
      catch (Exception) { }
    }

    #endregion

  }
}
