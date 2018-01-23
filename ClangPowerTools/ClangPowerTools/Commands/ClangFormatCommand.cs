using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Windows.Interop;
using System.Windows.Threading;
using ClangPowerTools.DialogPages;
using ClangPowerTools.SilentFile;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace ClangPowerTools.Commands
{
  /// <summary>
  /// Command handler
  /// </summary>
  internal sealed class ClangFormatCommand : ClangCommand
  {
    #region Members

    private ClangFormatPage mClangFormatPage;
    private bool mFormatAllActiveDocuments = false;
    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="ClangFormatCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    public ClangFormatCommand(Package aPackage, Guid aGuid, int aId) : base(aPackage, aGuid, aId)
    {
      mClangFormatPage = (ClangFormatPage)Package.GetDialogPage(typeof(ClangFormatPage));

      if (ServiceProvider.GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
      {
        var menuCommandID = new CommandID(CommandSet, Id);
        var menuCommand = new OleMenuCommand(this.RunClangFormat, menuCommandID);
        menuCommand.BeforeQueryStatus += mCommandsController.QueryCommandHandler;
        menuCommand.Enabled = true;
        commandService.AddCommand(menuCommand);
      }
    }

    #endregion

    #region Public methods

    public void DocumentOnSave(Document aDocument)
    {
      try
      {
        var dispatcher = HwndSource.FromHwnd((IntPtr)DTEObj.MainWindow.HWnd).RootVisual.Dispatcher;
        dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
        {
          RunClangFormat(new object(), new EventArgs());
        }));
      }
      catch (Exception) { }
    }

    public void OnBuildBegin(vsBuildScope Scope, vsBuildAction Action) => mFormatAllActiveDocuments = true;

    #endregion

    #region Private methods

    /// <summary>
    /// This function is the callback used to execute the command when the menu item is clicked.
    /// See the constructor to see how the menu item is associated with this function using
    /// OleMenuCommandService service and MenuCommand class.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event args.</param>
    private void RunClangFormat(object sender, EventArgs e)
    {
      var task = System.Threading.Tasks.Task.Run(() =>
      {
        try
        {
          SaveActiveDocuments();
          var selectedItems = CollectSelectedItems(mClangFormatPage.FileExtensions, mClangFormatPage.SkipFiles);

          var silentFileController = new SilentFileController();
          using (var guard = silentFileController.GetSilentFileChangerGuard())
          {
            mFileCollector = new FilePathCollector();
            var filesPath = mFileCollector.Collect(selectedItems).ToList();

            if (mFormatAllActiveDocuments)
              filesPath.AddRange(mFileCollector.Collect(DTEObj.Documents));

            mFormatAllActiveDocuments = false;

            silentFileController.SilentFiles(Package, guard, filesPath);
            RunScript(mClangFormatPage, filesPath);
          }
        }
        catch (Exception exception)
        {
          VsShellUtilities.ShowMessageBox(Package, exception.Message, "Error",
            OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
      });
    }

    #endregion

  }
}
