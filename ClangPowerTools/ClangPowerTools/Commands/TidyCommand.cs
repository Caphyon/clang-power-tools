using ClangPowerTools.DialogPages;
using ClangPowerTools.Output;
using ClangPowerTools.SilentFile;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Linq;

namespace ClangPowerTools
{
  /// <summary>
  /// Command handler
  /// </summary>
  internal sealed class TidyCommand : ClangCommand
  {
    #region Members

    private ClangTidyOptionsView mTidyOptions;
    private ClangTidyPredefinedChecksOptionsView mTidyChecks;
    private ClangTidyCustomChecksOptionsView mTidyCustomChecks;
    private ClangFormatOptionsView mClangFormatView;

    private bool mFix = false;

    private bool mForceTidyToFix = false;
    private bool mSaveCommandWasGiven = false;

    #endregion


    #region Properties


    /// <summary>
    /// Gets the instance of the command.
    /// </summary>
    public static TidyCommand Instance
    {
      get;
      private set;
    }


    #endregion


    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="TidyCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>

    private TidyCommand(OleMenuCommandService aCommandService, CommandsController aCommandsController, ErrorWindowController aErrorWindow, 
      OutputWindowController aOutputWindow, IVsSolution aSolution, DTE2 aDte, AsyncPackage aPackage, Guid aGuid, int aId)
        : base(aCommandsController, aErrorWindow, aOutputWindow, aSolution, aDte, aPackage, aGuid, aId)
    {
      mTidyOptions = (ClangTidyOptionsView)AsyncPackage.GetDialogPage(typeof(ClangTidyOptionsView));
      mTidyChecks = (ClangTidyPredefinedChecksOptionsView)AsyncPackage.GetDialogPage(typeof(ClangTidyPredefinedChecksOptionsView));
      mTidyCustomChecks = (ClangTidyCustomChecksOptionsView)AsyncPackage.GetDialogPage(typeof(ClangTidyCustomChecksOptionsView));
      mClangFormatView = (ClangFormatOptionsView)AsyncPackage.GetDialogPage(typeof(ClangFormatOptionsView));

      FileOpener.Initialize(DTEObj);

      if (null != aCommandService)
      {
        var menuCommandID = new CommandID(CommandSet, Id);
        var menuCommand = new OleMenuCommand(this.RunClangTidy, menuCommandID);
        menuCommand.BeforeQueryStatus += mCommandsController.OnBeforeClangCommand;
        menuCommand.Enabled = true;
        aCommandService.AddCommand(menuCommand);
      }
    }


    #endregion


    #region Public Methods


    /// <summary>
    /// Initializes the singleton instance of the command.
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    public static async System.Threading.Tasks.Task InitializeAsync(CommandsController aCommandsController, ErrorWindowController aErrorWindow, 
      OutputWindowController aOutputWindow, IVsSolution aSolution, DTE2 aDte, AsyncPackage aPackage, Guid aGuid, int aId)
    {
      // Switch to the main thread - the call to AddCommand in TidyCommand's constructor requires
      // the UI thread.
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(aPackage.DisposalToken);

      OleMenuCommandService commandService = await aPackage.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
      Instance = new TidyCommand(commandService, aCommandsController, aErrorWindow, aOutputWindow, aSolution, aDte, aPackage, aGuid, aId);
    }


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
    private async void RunClangTidy(object sender, EventArgs e)
    {
      if (mCommandsController.Running)
        return;

      mCommandsController.Running = true;
      mFix = SetTidyFixParameter(sender);

      // Switch to the UI thread 
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(AsyncPackage.DisposalToken);

      // Get Vs Running Document Table service async
      var vsRunningDocumentTable = await AsyncPackage.GetServiceAsync(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable;

      // Get Vs File Change service async
      var vsFileChange = await AsyncPackage.GetServiceAsync(typeof(SVsFileChangeEx)) as IVsFileChangeEx;

      await System.Threading.Tasks.Task.Run(() =>
      {
        try
        {
          DocumentsHandler.SaveActiveDocuments((DTE)DTEObj);
          AutomationUtil.SaveDirtyProjects(DTEObj.Solution);

          CollectSelectedItems(ScriptConstants.kAcceptedFileExtensions);

          using (var silentFileController = new SilentFileChangerController(vsRunningDocumentTable, vsFileChange))
          {
            using (var fileChangerWatcher = new FileChangerWatcher())
            {
              if (mFix || mTidyOptions.AutoTidyOnSave)
              {
                fileChangerWatcher.OnChanged += FileOpener.Open;

                string solutionFolderPath = DTEObj.Solution.FullName
                  .Substring(0, DTEObj.Solution.FullName.LastIndexOf('\\'));

                fileChangerWatcher.Run(solutionFolderPath);

                FilePathCollector fileCollector = new FilePathCollector();
                var filesPath = fileCollector.Collect(mItemsCollector.GetItems).ToList();

                silentFileController.SilentFiles(filesPath);
                silentFileController.SilentFiles(DTEObj.Documents);
              }
              RunScript(OutputWindowConstants.kTidyCodeCommand, mTidyOptions, mTidyChecks, mTidyCustomChecks, mClangFormatView, mFix);
            }
          }
        }
        catch (Exception exception)
        {
          VsShellUtilities.ShowMessageBox(AsyncPackage, exception.Message, "Error",
            OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
        finally
        {
          mForceTidyToFix = false;
        }
      });

      mCommandsController.OnAfterClangCommand();
    }

    private bool SetTidyFixParameter(object sender)
    {
      if (!(sender is OleMenuCommand))
        return false;

      return (sender as OleMenuCommand).CommandID.ID == CommandIds.kTidyFixId;
    }


    #endregion

  }
}
