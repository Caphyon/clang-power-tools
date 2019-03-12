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
using System.Windows.Threading;

namespace ClangPowerTools.Commands
{
  /// <summary>
  /// Command handler
  /// </summary>
  internal sealed class TidyCommand : ClangCommand
  {
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
      AsyncPackage aPackage, Guid aGuid, int aId)
        : base(aPackage, aGuid, aId)
    {
      if (null != aCommandService)
      {
        var menuCommandID = new CommandID(CommandSet, Id);
        var menuCommand = new OleMenuCommand(aCommandsController.Execute, menuCommandID);
        menuCommand.BeforeQueryStatus += aCommandsController.OnBeforeClangCommand;
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
      AsyncPackage aPackage, Guid aGuid, int aId)
    {
      // Switch to the main thread - the call to AddCommand in TidyCommand's constructor requires
      // the UI thread.
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(aPackage.DisposalToken);

      OleMenuCommandService commandService = await aPackage.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
      Instance = new TidyCommand(commandService, aCommandsController, aPackage, aGuid, aId);
    }


    public System.Threading.Tasks.Task RunClangTidyAsync(int aCommandId)
    {
      return System.Threading.Tasks.Task.Run(() =>
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
              var tidySettings = SettingsProvider.TidySettings;

              if (CommandIds.kTidyFixId == aCommandId || tidySettings.AutoTidyOnSave)
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
              RunScript(aCommandId);
            }
          }
        }
        catch (Exception exception)
        {
          VsShellUtilities.ShowMessageBox(AsyncPackage, exception.Message, "Error",
            OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
      });
    }

    #endregion

  }
}
