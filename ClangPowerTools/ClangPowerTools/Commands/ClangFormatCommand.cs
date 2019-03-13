using ClangPowerTools.DialogPages;
using ClangPowerTools.Output;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.ComponentModel.Design;
using System.IO;
using System.Xml.Linq;

namespace ClangPowerTools.Commands
{
  /// <summary>
  /// Command handler
  /// </summary>
  internal sealed class ClangFormatCommand : ClangCommand
  {
    #region Members

    private ClangFormatOptionsView mClangFormatView = null;
    private Document mDocument = null;

    #endregion


    #region Properties


    /// <summary>
    /// Gets the instance of the command.
    /// </summary>
    public static ClangFormatCommand Instance
    {
      get;
      private set;
    }


    #endregion


    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="ClangFormatCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    private ClangFormatCommand(OleMenuCommandService aCommandService, CommandsController aCommandsController, 
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


    #region Public methods


    /// <summary>
    /// Initializes the singleton instance of the command.
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    public static async System.Threading.Tasks.Task InitializeAsync(CommandsController aCommandsController, 
      AsyncPackage aPackage, Guid aGuid, int aId)
    {
      // Switch to the main thread - the call to AddCommand in ClangFormatCommand's constructor requires
      // the UI thread.
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(aPackage.DisposalToken);

      OleMenuCommandService commandService = await aPackage.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
      Instance = new ClangFormatCommand(commandService, aCommandsController, aPackage, aGuid, aId);
    }



    public void FormatDocument(Document aDocument, ClangFormatOptionsView aOptions)
    {
      mClangFormatView = aOptions;
      mDocument = aDocument;

      RunClangFormat();
    }


    public void RunClangFormat()
    {
      try
      {
        if (null == mClangFormatView)
        {
          FormatAllSelectedDocuments();
          return;
        }

        if (ScriptConstants.kCMakeConfigFile == mDocument.Name.ToLower())
          return;

        var view = Vsix.GetDocumentView(mDocument);
        if (view == null)
          return;

        StatusBarHandler.Status("Clang-Format started...", 1, vsStatusAnimation.vsStatusAnimationBuild, 1);

        System.Diagnostics.Process process;
        var dirPath = string.Empty;
        var filePath = Vsix.GetDocumentPath(view);
        var text = view.TextBuffer.CurrentSnapshot.GetText();

        var startPosition = 0;
        var length = text.Length;

        if (false == view.Selection.StreamSelectionSpan.IsEmpty)
        {
          // get the necessary elements for format selection
          FindStartPositionAndLengthOfSelectedText(view, text, out startPosition, out length);
          dirPath = Vsix.GetDocumentParent(view);
          mClangFormatView = SettingsProvider.ClangFormatSettings;
        }
        else
        {
          // format the end of the file for format document
          text = FormatEndOfFile(view, filePath, out dirPath);
        }

        process = CreateProcess(text, startPosition, length, dirPath, filePath, mClangFormatView);

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
        VsShellUtilities.ShowMessageBox(AsyncPackage, exception.Message, "Error while running clang-format",
          OLEMSGICON.OLEMSGICON_INFO, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
      }
      finally
      {
        mDocument = null;
        mClangFormatView = null;
        StatusBarHandler.Status("Clang-Format finished", 0, vsStatusAnimation.vsStatusAnimationBuild, 0);
      }
    }


    private void FormatAllSelectedDocuments()
    {
      foreach (var item in CollectSelectedItems(true))
      {
        var document = (item.GetObject() as ProjectItem).Document;

        if (null == document)
          document = DocumentsHandler.GetActiveDocument();

        mClangFormatView = SettingsProvider.ClangFormatSettings;
        mDocument = document;

        RunClangFormat();
      }
    }


    private string FormatEndOfFile(IWpfTextView aView, string aFilePath, out string aDirPath)
    {
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


    private void FindStartPositionAndLengthOfSelectedText(IWpfTextView aView, string aText, out int aStartPosition, out int aLength)
    {
      aStartPosition = aView.Selection.Start.Position.GetContainingLine().Start.Position;
      int end = aView.Selection.End.Position.GetContainingLine().End.Position;
      aLength = end - aStartPosition;

      // formatting a range that starts at the end of the file is not supported.
      if (aStartPosition >= aText.Length && aText.Length > 0)
        aStartPosition = aText.Length - 1;
    }


    private System.Diagnostics.Process CreateProcess(string aText, int aOffset, int aLength, string aPath, string aFilePath, ClangFormatOptionsView aClangFormatView)
    {
      string vsixPath = Path.GetDirectoryName(
        typeof(RunClangPowerToolsPackage).Assembly.Location);

      System.Diagnostics.Process process = new System.Diagnostics.Process();
      process.StartInfo.UseShellExecute = false;
      process.StartInfo.CreateNoWindow = true;
      process.StartInfo.RedirectStandardInput = true;
      process.StartInfo.RedirectStandardOutput = true;
      process.StartInfo.RedirectStandardError = true;
      process.StartInfo.FileName =
        true == (null != aClangFormatView.ClangFormatPath && aClangFormatView.ClangFormatPath.Enable && !string.IsNullOrWhiteSpace(aClangFormatView.ClangFormatPath.Value)) ?
          aClangFormatView.ClangFormatPath.Value : Path.Combine(vsixPath, ScriptConstants.kClangFormat);

      process.StartInfo.Arguments = " -offset " + aOffset +
                                    " -length " + aLength +
                                    " -output-replacements-xml " +
                                    $" {ScriptConstants.kStyle} \"{aClangFormatView.Style}\"" +
                                    $" {ScriptConstants.kFallbackStyle} \"{aClangFormatView.FallbackStyle}\"";

      var assumeFilename = aClangFormatView.AssumeFilename;
      if (string.IsNullOrEmpty(assumeFilename))
        assumeFilename = aFilePath;
      if (!string.IsNullOrEmpty(assumeFilename))
        process.StartInfo.Arguments += $" -assume-filename \"{assumeFilename}\"";

      if (null != aPath)
        process.StartInfo.WorkingDirectory = aPath;

      return process;
    }

    private void ApplyClangFormat(string replacements, IWpfTextView view)
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
