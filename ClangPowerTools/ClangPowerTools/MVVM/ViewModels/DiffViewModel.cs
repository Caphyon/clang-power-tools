using ClangPowerTools.Helpers;
using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.MVVM.Controllers;
using ClangPowerTools.MVVM.Interfaces;
using ClangPowerTools.MVVM.Models;
using ClangPowerTools.MVVM.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
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
    private DetectingView detectingView;
    private List<IFormatOption> detectedOptions;
    private List<IFormatOption> defaultOptions;
    private List<(FlowDocument, FlowDocument)> flowDocuments;
    private List<string> filesContent;
    private ICommand createFormatFileCommand;
    private ICommand reloadCommand;
    private ICommand resetCommand;
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
        OnPropertyChanged("SelectedOption");
      }
    }
    public bool CanExecute => true;

    #endregion

    #region Constructor 
    public DiffViewModel(DiffWindow diffWindow)
    {
      this.diffWindow = diffWindow;
      diffWindow.Closed += DiffWindow_Closed;
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
    public ICommand ResetCommand
    {
      get => resetCommand ??= new RelayCommand(() => ResetToDetectedOptionsAsync().SafeFireAndForget(), () => CanExecute);
    }

    private async Task ResetToDetectedOptionsAsync()
    {
      await Task.Run(() =>
      {
        formatStyleOptions = FormatOptionsProvider.CloneDetectedOptions(detectedOptions);
      });
      await ReloadDiffAsync();
      SelectedOption = FormatOptions.First();
      diffWindow.ReloadButton.IsEnabled = false;
      OnPropertyChanged("FormatOptions");
    }

    #endregion

    #region Public Methods

    public async Task DiffDocumentsAsync(List<string> filesPath)
    {
      ShowDetectingView();

      diffController.CancellationSource = new CancellationTokenSource();
      diffController.CancelTokenDisposed = false;
      CancellationToken cancelToken = diffController.CancellationSource.Token;
      try
      {
        filesContent = FileSystem.ReadContentFromMultipleFiles(filesPath, Environment.NewLine);
        (SelectedStyle, FormatOptions) = await diffController.GetFormatOptionsAsync(filesContent, cancelToken);
        SelectedOption = FormatOptions.First();
        ChangeOptionsFontWeight(FormatConstants.BoldFontWeight);
        flowDocuments = await diffController.CreateFlowDocumentsAsync(filesContent, SelectedStyle, FormatOptions, cancelToken);
        detectedOptions = FormatOptionsProvider.CloneDetectedOptions(FormatOptions);
        defaultOptions = FormatOptionsProvider.GetDefaultOptionsForStyle(SelectedStyle);
      }
      catch (OperationCanceledException)
      {
      }
      finally
      {
        diffController.CancelTokenDisposed = true;
        diffController.CancellationSource.Dispose();
      }

      DetectionFinished(filesPath);
    }

    public void OpenMultipleInput(int index)
    {
      SelectedOption = FormatOptions[index];
      OpenMultipleInput(SelectedOption);
    }

    public void OptionChanged(int index)
    {
      var option = formatStyleOptions[index];
      var defaultOption = defaultOptions[index];
      if (diffController.IsOptionChanged(option, defaultOption))
      {
        MarkOptionChange((FormatOptionModel)option, true, FormatConstants.BoldFontWeight);
      }
      else
      {
        MarkOptionChange((FormatOptionModel)option, false, FormatConstants.NormalFontWeight);
      }
      diffWindow.ReloadButton.IsEnabled = true;
    }

    public void ResetOption(int index)
    {
      var option = formatStyleOptions[index];
      var defaultOption = defaultOptions[index];
      diffController.CopyOptionValues(option, defaultOption);
      MarkOptionChange((FormatOptionModel)option, false, FormatConstants.NormalFontWeight);
    }

    #endregion

    #region Private Methods

    private void InitializeDiffView(List<string> filePaths)
    {
      FileNames = diffController.GetFileNames(filePaths);
      SetFlowDocuments();
      Style = "Base Style: " + SelectedStyle.ToString();

      OnPropertyChanged("FileNames");
      OnPropertyChanged("FormatOptions");
      OnPropertyChanged("Style");
    }

    private async Task ReloadDiffAsync()
    {
      ShowDetectingView();
      diffWindow.IsEnabled = false;

      diffController.CancellationSource = new CancellationTokenSource();
      diffController.CancelTokenDisposed = false;
      CancellationToken cancelToken = diffController.CancellationSource.Token;
      try
      {
        flowDocuments = await diffController.CreateFlowDocumentsAsync(filesContent, SelectedStyle, FormatOptions, cancelToken);
        SetFlowDocuments();
      }
      catch (OperationCanceledException)
      {
      }
      finally
      {
        diffController.CancelTokenDisposed = true;
        diffController.CancellationSource.Dispose();
      }

      //TODO find a better solution
      await Task.Delay(2000);
      diffWindow.IsEnabled = true;
      CloseDetectionView();
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

    private void DetectionFinished(List<string> filesPath)
    {
      if (detectingView.IsLoaded)
      {
        InitializeDiffView(filesPath);
        CloseDetectionView();
        diffWindow.Show();

        DetectedFormatStyleInfo info = new DetectedFormatStyleInfo(diffWindow);
        info.ShowDialog();
      }
    }

    private void ShowDetectingView()
    {
      detectingView = new DetectingView();
      detectingView.Show();
      detectingView.Closed += diffController.CloseLoadDetectionView;
    }

    private void CloseDetectionView()
    {
      detectingView.Closed -= diffController.CloseLoadDetectionView;
      detectingView.Close();
    }

    private void ChangeOptionsFontWeight(string fontWeight)
    {
      foreach (var item in formatStyleOptions)
      {
        var option = (FormatOptionModel)item;
        if (option.IsModifed)
        {
          option.NameFontWeight = fontWeight;
        }
      }
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

    private void MarkOptionChange(FormatOptionModel option, bool isModified, string fontWeight)
    {
      option.NameFontWeight = fontWeight;
      option.IsModifed = isModified;
    }

    private void DiffWindow_Closed(object sender, EventArgs e)
    {
      diffController.DeleteFormatFolder();
      diffWindow.Closed -= DiffWindow_Closed;
    }

    private void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
  }
}