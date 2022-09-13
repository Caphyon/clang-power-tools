using ClangPowerTools.Events;
using ClangPowerTools.Helpers;
using ClangPowerTools.IgnoreActions;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using Process = System.Diagnostics.Process;
using Task = System.Threading.Tasks.Task;

namespace ClangPowerTools.Commands
{
  /// <summary>
  /// Command handler
  /// </summary>
  public sealed class FormatCommand : ClangCommand
  {
    #region Members
    public event EventHandler<FormatCommandEventArgs> FormatEvent;

    private bool clearOutput = false;
    private Document mDocument = null;

    #endregion


    #region Properties


    /// <summary>
    /// Gets the instance of the command.
    /// </summary>
    public static FormatCommand Instance
    {
      get;
      private set;
    }


    #endregion


    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="FormatCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    private FormatCommand(OleMenuCommandService aCommandService, CommandController aCommandController,
      AsyncPackage aPackage, Guid aGuid, int aId)
        : base(aPackage, aGuid, aId)
    {
      if (null != aCommandService)
      {
        var menuCommandID = new CommandID(CommandSet, Id);
        var menuCommand = new OleMenuCommand(aCommandController.Execute, menuCommandID);
        menuCommand.BeforeQueryStatus += aCommandController.OnBeforeClangCommand;
        menuCommand.Enabled = true;
        aCommandService.AddCommand(menuCommand);
      }
    }

    #endregion


    #region Methods


    /// <summary>
    /// Initializes the singleton instance of the command.
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    public static async Task InitializeAsync(CommandController aCommandController,
      AsyncPackage aPackage, Guid aGuid, int aId)
    {
      // Switch to the main thread - the call to AddCommand in ClangFormatCommand's constructor requires
      // the UI thread.
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(aPackage.DisposalToken);

      OleMenuCommandService commandService = await aPackage.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
      Instance = new FormatCommand(commandService, aCommandController, aPackage, aGuid, aId);
    }

    public void RunClangFormat(CommandUILocation commandUILocation)
    {
      if (clearOutput == false)
        OnFormatFile(new FormatCommandEventArgs() { CanFormat = true, Clear = clearOutput });

      clearOutput = true;

      if (CommandUILocation.ContextMenu == commandUILocation)
      {
        FormatAllSelectedDocuments();
      }
      else
      {
        FormatActiveDocument();
      }
    }

    private void FormatActiveDocument()
    {
      mDocument = DocumentHandler.GetActiveDocument();
      ExecuteFormatCommand();
    }

    public void FormatOnSave(Document document)
    {
      clearOutput = true;
      mDocument = document;
      ExecuteFormatCommand();
    }

    public string RunFormatProcess(string dirPath, string filePath, string text, int startPosition, int length)
    {
      var process = CreateProcess(text, startPosition, length, dirPath, filePath);

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
      return output;
    }

    private void ExecuteFormatCommand()
    {
      try
      {
        if (ValidExecution(out IWpfTextView view) == false)
          return;

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
        }
        else
        {
          // format the end of the file for format document
          text = FormatEndOfFile(view, filePath, out dirPath);
        }

        string output = RunFormatProcess(dirPath, filePath, text, startPosition, length);

        ApplyClangFormat(output, view);
      }
      catch (Exception exception)
      {
        VsShellUtilities.ShowMessageBox(AsyncPackage, exception.Message, "Error while running clang-format",
          OLEMSGICON.OLEMSGICON_INFO, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
      }
    }


    #region Validation

    private bool ValidExecution(out IWpfTextView view)
    {
      var formatSettings = SettingsProvider.FormatSettingsModel;

      view = Vsix.GetDocumentView(mDocument);
      if (view == null)
        return false;

      if (IsFileStyleSelected(formatSettings))
      {
        var fileToFormatPath = Vsix.GetDocumentParent(view);
        if (!FileSystem.SearchAllTopDirectories(fileToFormatPath, FileSystem.ConfigClangFormatFileTypes))
        {
          OnFormatFile(new FormatCommandEventArgs()
          {
            Clear = clearOutput,
            FormatConfigFound = false
          });

          if (clearOutput)
            clearOutput = false;

          return false;
        }
      }

      if (FileHasExtension(mDocument.FullName, formatSettings.FileExtensions) == false)
      {
        OnFormatFile(new FormatCommandEventArgs()
        {
          IgnoreExtension = true,
          FileName = mDocument.Name,
          Clear = clearOutput
        });

        if (clearOutput)
          clearOutput = false;

        return false;
      }

      var ignoreItem = new IgnoreItem();

      if (ignoreItem.Check(mDocument))
      {
        OnFormatFile(new FormatCommandEventArgs()
        {
          IgnoreFile = true,
          FileName = mDocument.Name,
          Clear = clearOutput
        });

        if (clearOutput)
          clearOutput = false;

        OnIgnoreItem(new ClangCommandMessageEventArgs(ignoreItem.IgnoreFormatMessage, false));

        return false;
      }

      if (ScriptConstants.kCMakeConfigFile == mDocument.Name.ToLower())
        return false;

      OnFormatFile(new FormatCommandEventArgs()
      {
        Clear = true
      });

      return true;
    }

    private void OnFormatFile(FormatCommandEventArgs e)
    {
      FormatEvent?.Invoke(this, e);
    }

    private bool IsFileStyleSelected(FormatSettingsModel formatSettings)
    {
      return formatSettings.Style == ClangFormatStyle.file && 
        formatSettings.FallbackStyle == ClangFormatFallbackStyle.none;
    }

    private bool FileHasExtension(string filePath, string fileExtensions)
    {
      var extensions = fileExtensions.ToLower().Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
      return extensions.Contains(Path.GetExtension(filePath).ToLower());
    }

    #endregion

    private void FormatAllSelectedDocuments()
    {
      ItemsCollector itemsCollector = new ItemsCollector();
      itemsCollector.CollectSelectedProjectItems();
      List<Document> activeDocs = DocumentHandler.GetListOfActiveDocuments();
      Document activeDocument = DocumentHandler.GetActiveDocument();

      foreach (var item in itemsCollector.Items)
      {
        try
        {
          var projectItem = item.GetObject() as ProjectItem;
          mDocument = projectItem.Open().Document;
          ExecuteFormatCommand();

          if (DocumentHandler.IsOpen(mDocument, activeDocs))
          {
            mDocument.Save();
          }
          else
          {
            mDocument.Close(vsSaveChanges.vsSaveChangesYes);
          }
        }
        catch (Exception e)
        {
          MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
          mDocument = null;
        }
      }
      if (activeDocument != null)
      {
        activeDocument.Activate();
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


    private Process CreateProcess(string aText, int aOffset, int aLength, string aPath, string aFilePath)
    {
      var formatSettings = SettingsProvider.FormatSettingsModel;
      string vsixPath = Path.GetDirectoryName(
        GetType().Assembly.Location);

      Process process = new Process();
      process.StartInfo.UseShellExecute = false;
      process.StartInfo.CreateNoWindow = true;
      process.StartInfo.RedirectStandardInput = true;
      process.StartInfo.RedirectStandardOutput = true;
      process.StartInfo.RedirectStandardError = true;
      process.StartInfo.FileName =
        true == (string.IsNullOrWhiteSpace(formatSettings.CustomExecutable) == false) ?
          formatSettings.CustomExecutable : Path.Combine(vsixPath, ScriptConstants.kClangFormat);

      process.StartInfo.Arguments = " -offset " + aOffset +
                                    " -length " + aLength +
                                    " -output-replacements-xml " +
                                    $" {ScriptConstants.kStyle} \"{formatSettings.Style}\"" +
                                    $" {ScriptConstants.kFallbackStyle} \"{formatSettings.FallbackStyle}\"";

      var assumeFilename = formatSettings.AssumeFilename;
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
