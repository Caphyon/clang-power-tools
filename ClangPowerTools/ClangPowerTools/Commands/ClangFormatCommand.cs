using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ClangPowerTools.DialogPages;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace ClangPowerTools.Commands
{
  /// <summary>
  /// Command handler
  /// </summary>
  internal sealed class ClangFormatCommand : ClangCommand
  {
    #region Members

    private ClangFormatPage mClangFormatPage = null;
    private Document mDocument = null;
    private bool mFormatIsRunning = false;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="ClangFormatCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    public ClangFormatCommand(Package aPackage, Guid aGuid, int aId) : base(aPackage, aGuid, aId)
    {
      if (ServiceProvider.GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
      {
        var menuCommandID = new CommandID(CommandSet, Id);
        var menuCommand = new OleMenuCommand(RunClangFormat, menuCommandID);
        menuCommand.BeforeQueryStatus += mCommandsController.QueryCommandHandler;
        menuCommand.Enabled = true;
        commandService.AddCommand(menuCommand);
      }
    }

    #endregion

    #region Public methods

    public void OnBeforeSave(object sender, Document aDocument)
    {
      var clangFormatOptionPage = GetUserOptions();

      if (false == clangFormatOptionPage.EnableFormatOnSave && false == mFormatIsRunning)
        return;

      if (!Vsix.IsDocumentDirty(aDocument))
        return;

      if (!FileHasExtension(aDocument.FullName, clangFormatOptionPage.FileExtensions))
        return;

      if (SkipFile(aDocument.FullName, clangFormatOptionPage.SkipFiles))
        return;

      mClangFormatPage = GetUserOptions();
      mDocument = aDocument;

      RunClangFormat(new object(), new EventArgs());
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
      try
      {
        List<Document> documents = new List<Document>();
        if (null == mDocument)
        {
          mFormatIsRunning = true;
          FormatAllSelectedDocuments();
          mFormatIsRunning = false;
          return;
        }

        var view = Vsix.GetDocumentView(mDocument);
        if (view == null)
          return;

        var text = FormatEndOfFile(view, out string dirPath, out string filePath);
        var process = CreateProcess(text, 0, text.Length, dirPath, filePath, mClangFormatPage);

        try
        {
          process.Start();
        }
        catch (Exception exception)
        {
          throw new Exception(
              $"Cannot execute {process.StartInfo.FileName}.\n{exception.Message}.");
        }

        process.StandardInput.Write(text);
        process.StandardInput.Close();

        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        if (0 != process.ExitCode)
          throw new Exception(process.StandardError.ReadToEnd());

        ApplyClangFormat(output, view);
      }
      catch (Exception exception)
      {
        VsShellUtilities.ShowMessageBox(Package, exception.Message, "Error",
          OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
      }
      finally
      {
        mDocument = null;
        mClangFormatPage = null;
      }
    }

    private ClangFormatPage GetUserOptions() => (ClangFormatPage)Package.GetDialogPage(typeof(ClangFormatPage));

    private bool SkipFile(string aFilePath, string aSkipFiles)
    {
      var skipFilesList = aSkipFiles.ToLower().Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
      return skipFilesList.Contains(Path.GetFileName(aFilePath).ToLower());
    }

    private bool FileHasExtension(string filePath, string fileExtensions)
    {
      var extensions = fileExtensions.ToLower().Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
      return extensions.Contains(Path.GetExtension(filePath).ToLower());
    }

    private void FormatAllSelectedDocuments()
    {
      foreach (var item in CollectSelectedItems())
      {
        if (!(item.GetObject() is ProjectItem))
          return; // the selected file is not a project item

        var document = (item.GetObject() as ProjectItem).Document;
        if (null == document)
          continue;

        document.Save(document.FullName);
        if (false == GetUserOptions().EnableFormatOnSave)
          OnBeforeSave(new object(), document);
      }
    }

    private string FormatEndOfFile(IWpfTextView aView, out string aDirPath, out string aFilePath)
    {
      aFilePath = Vsix.GetDocumentPath(aView);
      aDirPath = Path.GetDirectoryName(aFilePath);

      var text = aView.TextBuffer.CurrentSnapshot.GetText();
      var newline = text.Contains(Environment.NewLine) ? Environment.NewLine : "\n";

      if (!text.EndsWith(newline))
      {
        aView.TextBuffer.Insert(aView.TextBuffer.CurrentSnapshot.Length, newline);
        text += newline;
      }

      return text;
    }

    private System.Diagnostics.Process CreateProcess(string aText, int aOffset, int aLength, string aPath, string aFilePath, ClangFormatPage aOptions)
    {
      string vsixPath = Path.GetDirectoryName(
        typeof(RunClangPowerToolsPackage).Assembly.Location);

      System.Diagnostics.Process process = new System.Diagnostics.Process();
      process.StartInfo.UseShellExecute = false;
      process.StartInfo.CreateNoWindow = true;
      process.StartInfo.RedirectStandardInput = true;
      process.StartInfo.RedirectStandardOutput = true;
      process.StartInfo.RedirectStandardError = true;
      process.StartInfo.FileName = Path.Combine(vsixPath, ScriptConstants.kClangFormat);

      process.StartInfo.Arguments = " -offset " + aOffset +
                                    " -length " + aLength +
                                    " -output-replacements-xml " +
                                    $" {ScriptConstants.kStyle} \"{aOptions.Style}\"" +
                                    $" {ScriptConstants.kFallbackStyle} \"{aOptions.FallbackStyle}\"";

      var assumeFilename = aOptions.AssumeFilename;
      if (string.IsNullOrEmpty(assumeFilename))
        assumeFilename = aFilePath;
      if (!string.IsNullOrEmpty(assumeFilename))
        process.StartInfo.Arguments += $" -assume-filename \"{assumeFilename}\"";

      if (null != aPath)
        process.StartInfo.WorkingDirectory = aPath;

      return process;
    }

    private static void ApplyClangFormat(string replacements, IWpfTextView view)
    {
      if (string.IsNullOrWhiteSpace(replacements))
        return;

      var root = XElement.Parse(replacements);
      var edit = view.TextBuffer.CreateEdit();
      foreach (XElement replacement in root.Descendants("replacement"))
      {
        var span = new Span(
            int.Parse(replacement.Attribute("offset").Value),
            int.Parse(replacement.Attribute("length").Value));
        edit.Replace(span, replacement.Value);
      }
      edit.Apply();
    }

    #endregion

  }
}
