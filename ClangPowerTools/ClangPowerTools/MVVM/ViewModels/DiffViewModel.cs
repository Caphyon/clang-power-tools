using ClangPowerTools.Helpers;
using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.MVVM.Controllers;
using ClangPowerTools.MVVM.Interfaces;
using ClangPowerTools.MVVM.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class DiffViewModel : CommonFormatEditorFunctionality, INotifyPropertyChanged, IFormatEditor
  {
    #region Members

    public event PropertyChangedEventHandler PropertyChanged;

    private readonly DiffWindow diffWindow;
    private readonly DiffController diffController;
    private List<IFormatOption> detectedOptions;
    private List<(FlowDocument, FlowDocument)> flowDocuments;
    private List<string> filesContent;
    private ICommand createFormatFileCommand;
    private ICommand reloadCommand;
    private string selectedFile;
    private const int PageWith = 1000;

    #endregion


    #region Properties

    public List<IFormatOption> FormatOptions
    {
      get => formatStyleOptions;
      set => formatStyleOptions = value;
    }
    public EditorStyles SelectedStyle
    {
      get => selectedStyle;
      set => selectedStyle = value;
    }
    public string Style { get; set; }
    public IEnumerable<ToggleValues> BooleanComboboxValues
    {
      get
      {
        return Enum.GetValues(typeof(ToggleValues)).Cast<ToggleValues>();
      }
    }
    public string OptionsFile { get; set; }
    public List<string> FileNames { get; set; }
    public int SelectedIndex { get; set; }
    public string SelectedFile
    {
      get
      {
        if (string.IsNullOrEmpty(selectedFile) && FileNames.Count > 0)
        {
          selectedFile = FileNames.First();
        }
        if (diffWindow.IsActive)
        {
          SetFlowDocuments();
        }
        return selectedFile;
      }
      set => selectedFile = value;
    }

    public IFormatOption SelectedOption
    {
      get
      {
        return selectedOption;
      }
      set
      {
        selectedOption = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedOption"));
      }
    }
    public bool CanExecute => true;

    #endregion

    #region Constructor 
    public DiffViewModel(DiffWindow diffWindow)
    {
      this.diffWindow = diffWindow;
      diffController = new DiffController();
      FileNames = new List<string>();
    }

    //Empty constructor used for XAML IntelliSense
    public DiffViewModel()
    {

    }

    #endregion

    #region Commands

    public ICommand CreateFormatFileCommand
    {
      get => createFormatFileCommand ??= new RelayCommand(() => CreateFormatFile(), () => CanExecute);
    }
    public ICommand ReloadCommand
    {
      get => reloadCommand ??= new RelayCommand(() => ReloadDiffAsync().SafeFireAndForget(), () => CanExecute);
    }
    public ICommand ResetCommand => throw new NotImplementedException();

    #endregion

    #region Public Methods

    public async Task ReloadDiffAsync()
    {
      var detectingView = new DetectingView
      {
        Owner = diffWindow
      };
      detectingView.Show();
      detectingView.Closed += diffController.CloseLoadingView;

      flowDocuments = await diffController.CreateFlowDocumentsAsync(filesContent, SelectedStyle, FormatOptions);
      SetFlowDocuments();

      detectingView.Closed -= diffController.CloseLoadingView;
      detectingView.Close();
    }

    public async Task DiffDocumentsAsync(List<string> filesPath)
    {
      DetectingView detectingView = ShowDetectingView();

      filesContent = FileSystem.ReadContentFromMultipleFiles(filesPath, Environment.NewLine);
      (SelectedStyle, FormatOptions) = await diffController.GetFormatOptionsAsync(filesContent);
      detectedOptions = new List<IFormatOption>(FormatOptions);
      flowDocuments = await diffController.CreateFlowDocumentsAsync(filesContent, SelectedStyle, FormatOptions);

      DetectionFinished(filesPath, detectingView);
    }

    public void OpenMultipleInput(int index)
    {
      CloseMultipleInput += FormatAfterClosingMultipleInput;
      SelectedOption = FormatOptions[index];
      OpenMultipleInput(SelectedOption);
    }

    public void FormatAfterClosingMultipleInput(object sender, EventArgs e)
    {
      CloseMultipleInput -= FormatAfterClosingMultipleInput;
      ReloadDiffAsync().SafeFireAndForget();
    }

    #endregion

    #region Private Methods

    private void InitializeDiffView(List<string> filePaths)
    {
      FileNames = diffController.GetFileNames(filePaths);
      SetFlowDocuments();
      Style = "Style: " + SelectedStyle.ToString();

      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FileNames"));
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FormatOptions"));
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Style"));
    }

    private void SetFlowDocuments()
    {
      FlowDocument diffInput;
      FlowDocument diffOutput;
      if (string.IsNullOrEmpty(selectedFile))
      {
        SelectedFile = FileNames.First();
      }
      diffInput = flowDocuments[SelectedIndex].Item1;
      diffOutput = flowDocuments[SelectedIndex].Item2;
      diffInput.PageWidth = PageWith;
      diffOutput.PageWidth = PageWith;
      diffWindow.DiffInput.Document = diffInput;
      diffWindow.DiffOutput.Document = diffOutput;
    }

    private void DetectionFinished(List<string> filesPath, DetectingView detectingView)
    {
      if (detectingView.IsLoaded)
      {
        InitializeDiffView(filesPath);
        detectingView.Closed -= diffController.CloseLoadingView;
        detectingView.Close();
        diffWindow.Show();
      }
    }

    private DetectingView ShowDetectingView()
    {
      var detectingView = new DetectingView();
      detectingView.Show();
      detectingView.Closed += diffController.CloseLoadingView;
      return detectingView;
    }

    private void CreateFormatFile()
    {
      string fileName = ".clang-format";
      string defaultExt = ".clang-format";
      string filter = "Configuration files (.clang-format)|*.clang-format";

      string path = SaveFile(fileName, defaultExt, filter);
      if (string.IsNullOrEmpty(path) == false)
      {
        WriteContentToFile(path, FormatOptionFile.CreateOutput(FormatOptions, SelectedStyle).ToString());
      }
    }

    #endregion

  }
}
