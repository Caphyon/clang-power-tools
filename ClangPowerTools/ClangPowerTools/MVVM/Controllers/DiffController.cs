using ClangPowerTools.DiffStyle;
using ClangPowerTools.Helpers;
using ClangPowerTools.MVVM.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace ClangPowerTools.MVVM.Controllers
{
  public class DiffController
  {
    #region Members

    private readonly StyleDetector styleDetector;

    #endregion

    #region Constructor

    public DiffController()
    {
      styleDetector = new StyleDetector();
    }

    public void CloseLoadingView(object sender, EventArgs e)
    {
      styleDetector.CancellationSource.Cancel();
    }

    #endregion

    #region Public Methods

    public async Task<(EditorStyles matchedStyle, List<IFormatOption> matchedOptions)> GetFormatOptionsAsync(List<string> filePaths)
    {
      return await styleDetector.DetectStyleOptionsAsync(filePaths);
    }

    public async Task<List<(FlowDocument, FlowDocument)>> CreateFlowDocumentsAsync(List<string> filePaths, EditorStyles formatStyle, List<IFormatOption> formatOptions)
    {
      var flowDocuments = new List<(FlowDocument, FlowDocument)>();

      foreach (var path in filePaths)
      {
        var input = FileSystem.ReadContentFromFile(path, Environment.NewLine);
        var documents = await CreateFlowDocumentAsync(input, formatStyle, formatOptions);
        flowDocuments.Add(documents);
      }
      return flowDocuments;
    }

    public async Task<(FlowDocument, FlowDocument)> CreateFlowDocumentAsync(string input, EditorStyles formatStyle, List<IFormatOption> formatOptions)
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

    public List<string> GetFileNames(List<string> filePaths)
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
