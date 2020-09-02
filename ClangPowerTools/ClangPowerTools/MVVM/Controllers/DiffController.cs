using ClangPowerTools.DiffStyle;
using ClangPowerTools.MVVM.Interfaces;
using ClangPowerTools.MVVM.Views;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
      var diffWindow = new DiffWindow(formatOptions, formatStyle, editorInput, filePaths, CreateFormatFile);
      await diffWindow.ShowDiffAsync();
      diffWindow.Show();
    }

    #endregion
  }
}
