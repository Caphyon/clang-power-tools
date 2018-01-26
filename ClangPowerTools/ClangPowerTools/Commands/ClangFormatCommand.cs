using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Windows.Interop;
using System.Windows.Threading;
using ClangPowerTools.DialogPages;
using ClangPowerTools.SilentFile;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;

namespace ClangPowerTools.Commands
{
  /// <summary>
  /// Command handler
  /// </summary>
  internal sealed class ClangFormatCommand : ClangCommand
  {
    #region Members

    private ClangFormatPage mClangFormatPage;
    private Commands2 mCommands;

    private Document mDocument = null;

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
      mCommands = DTEObj.Commands as Commands2;

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

    public void OnBeforeSave(object sender, Document document)
    {
      if (!mClangFormatPage.EnableFormatOnSave)
        return;

      if (!Vsix.IsDocumentDirty(document))
        return;

      try
      {
        var dispatcher = HwndSource.FromHwnd((IntPtr)DTEObj.MainWindow.HWnd).RootVisual.Dispatcher;
        dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
        {
          mDocument = document;
          FormatEndFile();
          RunClangFormat(new object(), new EventArgs());
        }));
      }
      catch (Exception) { }
    }
   
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
          // save the active document when the command was given from the clang-format button
          // the save event will trigger the OnBeforeSave event handler
          if (null == mDocument)
          {
            DocumentsHandler.SaveActiveDocuments((DTE)DTEObj);
            return;
          }

          var silentFileController = new SilentFileController();
          using (var guard = silentFileController.GetSilentFileChangerGuard())
          {
            mFilePahtCollector = new FilePathCollector();
            List<string> filesPath = new List<string>
            {
              mFilePahtCollector.Collect(mDocument)
            };

            silentFileController.SilentFiles(Package, guard, filesPath);
            RunScript(mClangFormatPage, filesPath);
          }
        }
        catch (Exception exception)
        {
          VsShellUtilities.ShowMessageBox(Package, exception.Message, "Error",
            OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
        finally
        {
          mDocument = null;
        }
      });
    }

    public void FormatEndFile()
    {
      IWpfTextView view = Vsix.GetCurrentView();

      string filePath = Vsix.GetDocumentPath(view);
      var path = Path.GetDirectoryName(filePath);

      string text = view.TextBuffer.CurrentSnapshot.GetText();

      string newline = text.Contains(Environment.NewLine) ? Environment.NewLine : "\n";
      if (!text.EndsWith(newline))
      {
        view.TextBuffer.Insert(view.TextBuffer.CurrentSnapshot.Length, newline);
        text += newline;
      }
    }



    #endregion

  }
}
