using ClangPowerTools.DiffStyle;
using ClangPowerTools.Helpers;
using ClangPowerTools.MVVM.Interfaces;
using ClangPowerTools.MVVM.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace ClangPowerTools.MVVM.Controllers
{
  public class DiffController
  {
    #region Members

    public EventHandler ClosedWindow;

    private readonly Action CreateFormatFile;
    private List<IFormatOption> formatOptions;
    private EditorStyles formatStyle;
    private string editorInput;
    private List<string> filePaths;
    private List<(FlowDocument, FlowDocument)> flowDocuments;

    #endregion

    #region Constructor

    public DiffController(Action CreateFormatFile)
    {
      StyleDetector.StopDetection = false;
      ClosedWindow += CloseLoadingView;
      this.CreateFormatFile = CreateFormatFile;
    }

    private void CloseLoadingView(object sender, EventArgs e)
    {
      StyleDetector.StopDetection = true;
      ClosedWindow -= CloseLoadingView;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Get the found EditorStyle and FormatOptions
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public async Task<(EditorStyles matchedStyle, List<IFormatOption> matchedOptions)> GetFormatOptionsAsync(string editorInput, List<string> filePaths)
    {
      this.editorInput = editorInput;
      this.filePaths = filePaths;

      var styleDetector = new StyleDetector();
      EditorStyles matchedStyle;
      List<IFormatOption> matchedOptions;
      if (filePaths.Count > 0)
      {
        (matchedStyle, matchedOptions) = await styleDetector.DetectStyleOptionsAsync(filePaths);
      }
      else
      {
        (matchedStyle, matchedOptions) = await styleDetector.DetectStyleOptionsAsync(editorInput);
      }

      formatStyle = matchedStyle;
      formatOptions = matchedOptions;
      return (formatStyle, formatOptions);
    }

    /// <summary>
    /// Display the diffs after GetFormatOptionsAsync
    /// </summary>
    /// <returns></returns>
    public async Task ShowDiffAsync()
    {
      await PopulateFlowDocumentsListAsync();
      string optionsFile = CleanOptionsFile();
      List<string> fileNames = GetFileNames();
      var diffWindow = new DiffWindow(flowDocuments, fileNames, optionsFile, CreateFormatFile);
      diffWindow.Show();
    }

    private async Task PopulateFlowDocumentsListAsync()
    {
      flowDocuments = new List<(FlowDocument, FlowDocument)>();
      if (filePaths.Count > 0)
      {
        foreach (var path in filePaths)
        {
          var input = FileSystem.ReadContentFromFile(path, Environment.NewLine);
          var documents = await CreateFlowDocumentAsync(input);
          flowDocuments.Add(documents);
        }
      }
      else
      {
        var documents = await CreateFlowDocumentAsync(editorInput);
        flowDocuments.Add(documents);
      }
    }

    private async Task<(FlowDocument, FlowDocument)> CreateFlowDocumentAsync(string input)
    {
      var diffMatchPatchWrapper = new DiffMatchPatchWrapper();
      string output = string.Empty;
      await Task.Run(() =>
      {
        var formatter = new StyleFormatter();
        output = formatter.FormatText(input, formatOptions, formatStyle);
        diffMatchPatchWrapper.Diff(input, output);
        diffMatchPatchWrapper.CleanupSemantic();
      });
      return diffMatchPatchWrapper.DiffAsFlowDocuments(input, output);
    }

    private string CleanOptionsFile()
    {
      var formatOptionFile = FormatOptionFile.CreateOutput(formatOptions, formatStyle).ToString();
      var lines = formatOptionFile.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
      var sb = new StringBuilder();
      for (int i = 2; i < lines.Length - 1; i++)
      {
        sb.AppendLine(lines[i]);
      }
      return sb.ToString();
    }

    private List<string> GetFileNames()
    {
      var fileNames = new List<string>();
      foreach (var path in filePaths)
      {
        fileNames.Add(Path.GetFileName(path));
      }
      return fileNames;
    }

    #endregion
  }
}
