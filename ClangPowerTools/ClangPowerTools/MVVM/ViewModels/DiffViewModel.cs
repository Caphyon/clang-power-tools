using ClangPowerTools.Helpers;
using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.MVVM.Interfaces;
using ClangPowerTools.MVVM.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class DiffViewModel
  {
    #region Members

    private readonly Action CreateFormatFile;
    private readonly DiffWindow diffWindow;
    private ICommand createFormatFileCommand;
    private readonly EditorStyles editorStyle;
    private readonly List<IFormatOption> formatOptions;
    private readonly List<string> filePaths;
    private string editorInput;
    private string selectedFile;

    private const int PageWith = 1000;

    #endregion


    #region Properties

    public string FormatFile { get; set; }

    public List<string> Files
    {
      get
      {
        return filePaths;
      }
    }

    public string SelectedFile
    {
      get
      {
        if (string.IsNullOrEmpty(selectedFile))
        {
          selectedFile = filePaths.First();
        }
        if (diffWindow.IsActive)
        {
          FileChangeDiffAsync().SafeFireAndForget();
        }
        return selectedFile;
      }
      set
      {
        selectedFile = value;
      }
    }

    public bool CanExecute
    {
      get
      {
        return true;
      }
    }

    #endregion

    #region Constructor 

    public DiffViewModel(DiffWindow diffWindow, List<IFormatOption> formatOptions, EditorStyles editorStyle, string editorInput, List<string> filePaths, Action CreateFormatFile)
    {
      FormatFile = CleanOptionFile(FormatOptionFile.CreateOutput(formatOptions, editorStyle).ToString());
      this.CreateFormatFile = CreateFormatFile;
      this.diffWindow = diffWindow;
      this.editorStyle = editorStyle;
      this.editorInput = editorInput;
      this.filePaths = filePaths;
      this.formatOptions = formatOptions;
    }

    //Empty constructor used for XAML IntelliSense
    public DiffViewModel()
    {

    }

    #endregion

    #region Commands

    public ICommand CreateFormatFileCommand
    {
      get => createFormatFileCommand ??= new RelayCommand(() => CreateFormatFile.Invoke(), () => CanExecute);
    }

    #endregion


    #region Public Methods

    public async Task ShowDiffAsync()
    {
      string input = GetInputToFormat();

      var (diffInput, diffOutput) = await CreateFlowDocumentsAsync(input);
      diffInput.PageWidth = PageWith;
      diffOutput.PageWidth = PageWith;
      diffWindow.DiffInput.Document = diffInput;
      diffWindow.DiffOutput.Document = diffOutput;
    }

    #endregion

    #region Private Methods

    private async Task FileChangeDiffAsync()
    {
      var loadingView = new DetectingView
      {
        Owner = diffWindow
      };
      loadingView.Show();
      diffWindow.IsEnabled = false;
      //loadingView.Closed += diffController.ClosedWindow;

      await ShowDiffAsync();

      loadingView.Close();
      diffWindow.IsEnabled = true;
      //if (loadingView.IsLoaded == false)
      //{
      //  diffWindow.IsEnabled = true;
      //  return;
      //}
    }

    private string CleanOptionFile(string formatOptionFile)
    {
      var lines = formatOptionFile.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
      var sb = new StringBuilder();
      for (int i = 2; i < lines.Length - 1; i++)
      {
        sb.AppendLine(lines[i]);
      }
      return sb.ToString();
    }

    private string GetInputToFormat()
    {
      string input;
      if (filePaths.Count > 0)
      {
        input = FileSystem.ReadContentFromFile(SelectedFile);
      }
      else
      {
        input = editorInput;
      }
      return input;
    }

    private async Task<(FlowDocument, FlowDocument)> CreateFlowDocumentsAsync(string input)
    {
      var diffMatchPatchWrapper = new DiffMatchPatchWrapper();
      string output = string.Empty;
      await Task.Run(() =>
       {
         var formatter = new StyleFormatter();
         output = formatter.FormatText(input, formatOptions, editorStyle);
         diffMatchPatchWrapper.Diff(input, output);
         diffMatchPatchWrapper.CleanupSemantic();
       });

      return diffMatchPatchWrapper.DiffAsFlowDocuments(input, output);
    }

    #endregion

  }
}
