using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using ClangPowerTools.DialogPages;
using EnvDTE80;
using ClangPowerTools.SilentFile;

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
    private FileChangerWatcher mFileWatcher;
    private FileOpener mFileOpener;

    private bool mForceTidyToFix = false;
    private bool mSaveCommandWasGiven = false;

    //private SilentFileChangerGuard mGuard = null;
    private Commands2 mCommand;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="TidyCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>

    public TidyCommand(Package aPackage, Guid aGuid, int aId) : base(aPackage, aGuid, aId)
    {
      try
      {
        mTidyOptions = (TidyOptions)Package.GetDialogPage(typeof(TidyOptions));
        mTidyChecks = (TidyChecks)Package.GetDialogPage(typeof(TidyChecks));
        mTidyCustomChecks = (TidyCustomChecks)Package.GetDialogPage(typeof(TidyCustomChecks));

        mCommand = DTEObj.Commands as Commands2;
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
      catch (Exception)
      {
      }
    }

    #endregion

    #region Public Methods

    public void OnBeforeSave(object sender, Document aDocument)
    {
      try
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
      }
      catch (Exception)
      {
      }
      finally
      {
        mSaveCommandWasGiven = false;
      }
    }

    public void CommandEventsBeforeExecute(string aGuid, int aId, object aCustomIn, object aCustomOut, ref bool aCancelDefault)
    {
      try
      {
        string commandName = GetCommandName(aGuid, aId);
        if (0 != string.Compare("File.SaveAll", commandName) &&
          0 != string.Compare("File.SaveSelectedItems", commandName))
        {
          return;
        }
        mSaveCommandWasGiven = true;
      }
      catch (Exception)
      {
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
    private void RunClangTidy(object sender, EventArgs e)
    {
      mCommandsController.Running = true;
      var task = System.Threading.Tasks.Task.Run(() =>
      {
        try
        {
          AutomationUtil.SaveDirtyFiles(Package, DTEObj.Solution, DTEObj);
          CollectSelectedItems();

          mFileWatcher = new FileChangerWatcher();
          var silentFileController = new SilentFileController();

          using (var guard = silentFileController.GetSilentFileChangerGuard())
          {
            if (true == mTidyOptions.Fix || true == mTidyOptions.AutoTidyOnSave)
            {
              WatchFiles();

              FilePathCollector fileCollector = new FilePathCollector();
              var filesPath = fileCollector.Collect(mItemsCollector.GetItems);

              silentFileController.SilentFiles(Package, guard, filesPath);
              silentFileController.SilentOpenFiles(Package, guard, DTEObj);
            }
            RunScript(OutputWindowConstants.kTidyCodeCommand, mForceTidyToFix, mTidyOptions, mTidyChecks, mTidyCustomChecks);
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
      }).ContinueWith(tsk => mCommandsController.AfterExecute()); ;
    }

    #endregion

    #region Helpers

    private void SilentFiles(SilentFileChangerGuard aGuard)
    {
      try
      {
        FilePathCollector fileCollector = new FilePathCollector();
        fileCollector.Collect(mItemsCollector.GetItems);

        // silent all open files
        foreach (Document doc in DTEObj.Documents)
          aGuard.Add(new SilentFileChanger(Package, Path.Combine(doc.Path, doc.Name), true));
        //silent all selected files
        aGuard.AddRange(Package, fileCollector.Files);
      }
      catch (Exception)
      {
      }

    }

    private void WatchFiles()
    {
      try
      {
        mFileWatcher.OnChanged += mFileOpener.FileChanged;
        string solutionFolderPath = DTEObj.Solution.FullName
          .Substring(0, DTEObj.Solution.FullName.LastIndexOf('\\'));
        mFileWatcher.Run(solutionFolderPath);
      }
      catch (Exception)
      {
      }
    }

    private string GetCommandName(string aGuid, int aId)
    {
      if (null == aGuid)
        return "null";

      if (null == mCommand)
        return string.Empty;

      try
      {
        return mCommand.Item(aGuid, aId).Name;
      }
      catch (Exception)
      {
      }

      return string.Empty;
    }

    #endregion

  }
}
