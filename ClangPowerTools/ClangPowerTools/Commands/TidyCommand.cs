using ClangPowerTools.DialogPages;
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

namespace ClangPowerTools.Commands
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

    private TidyCommand(OleMenuCommandService aCommandService, CommandsController aCommandsController, 
      ErrorWindowController aErrorWindow, OutputWindowController aOutputWindow, AsyncPackage aPackage, Guid aGuid, int aId)
        : base(aCommandsController, aErrorWindow, aOutputWindow, aPackage, aGuid, aId)
    {
      mTidyOptions = (ClangTidyOptionsView)AsyncPackage.GetDialogPage(typeof(ClangTidyOptionsView));
      mTidyChecks = (ClangTidyPredefinedChecksOptionsView)AsyncPackage.GetDialogPage(typeof(ClangTidyPredefinedChecksOptionsView));
      mTidyCustomChecks = (ClangTidyCustomChecksOptionsView)AsyncPackage.GetDialogPage(typeof(ClangTidyCustomChecksOptionsView));
      mClangFormatView = (ClangFormatOptionsView)AsyncPackage.GetDialogPage(typeof(ClangFormatOptionsView));

      if (null != aCommandService)
      {
        var menuCommandID = new CommandID(CommandSet, Id);
        var menuCommand = new OleMenuCommand(mCommandsController.Execute, menuCommandID);
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
    public static async System.Threading.Tasks.Task InitializeAsync(CommandsController aCommandsController, 
      ErrorWindowController aErrorWindow, OutputWindowController aOutputWindow, AsyncPackage aPackage, Guid aGuid, int aId)
    {
      // Switch to the main thread - the call to AddCommand in TidyCommand's constructor requires
      // the UI thread.
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(aPackage.DisposalToken);

      OleMenuCommandService commandService = await aPackage.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
      Instance = new TidyCommand(commandService, aCommandsController, aErrorWindow, aOutputWindow, aPackage, aGuid, aId);
    }


    public void RunClangTidy(bool aTidyFix = false)
    {
      if (mCommandsController.Running)
        return;

      mCommandsController.Running = true;
      mFix = aTidyFix;

      var task = System.Threading.Tasks.Task.Run(() =>
      {
        try
        {
          DocumentsHandler.SaveActiveDocuments();

          if (!VsServiceProvider.TryGetService(typeof(DTE), out object dte))
            return;

          var dte2 = dte as DTE2;
          AutomationUtil.SaveDirtyProjects(dte2.Solution);

          CollectSelectedItems(false, ScriptConstants.kAcceptedFileExtensions);

          using (var silentFileController = new SilentFileChangerController())
          {
            using (var fileChangerWatcher = new FileChangerWatcher())
            {
              if (mFix || mTidyOptions.AutoTidyOnSave)
              {
                fileChangerWatcher.OnChanged += FileOpener.Open;

                string solutionFolderPath = dte2.Solution.FullName
                  .Substring(0, dte2.Solution.FullName.LastIndexOf('\\'));

                fileChangerWatcher.Run(solutionFolderPath);

                FilePathCollector fileCollector = new FilePathCollector();
                var filesPath = fileCollector.Collect(mItemsCollector.GetItems).ToList();

                silentFileController.SilentFiles(filesPath);
                silentFileController.SilentFiles(dte2.Documents);
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
      }).ContinueWith(tsk => mCommandsController.OnAfterClangCommand());
    }




    #endregion

  }
}
