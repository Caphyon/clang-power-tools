using ClangPowerTools.Helpers;
using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.MVVM.Controllers;
using ClangPowerTools.MVVM.Interfaces;
using ClangPowerTools.MVVM.Models;
using ClangPowerTools.MVVM.Views;
using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using Brushes = System.Windows.Media.Brushes;

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
    private int multipleInputDataIndex;

    private DetectedFormatStyleInfo infoWindow;

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
      get => reloadCommand ??= new RelayCommand(() => ReloadDiffAsync(FormatConstants.UpdateTitle, FormatConstants.UpdateDescription, string.Empty).SafeFireAndForget(), () => CanExecute);
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
      await ReloadDiffAsync(FormatConstants.ResetTitle, FormatConstants.ResetDescription, string.Empty);
      SelectedOption = FormatOptions.First();
      diffWindow.ReloadButton.IsEnabled = false;
      OnPropertyChanged("FormatOptions");
    }

    #endregion

    #region Public Methods

    public async Task DiffDocumentsAsync(List<string> filesPath, DialogWindow detectingWindowOwner)
    {
      ShowDetectingView(detectingWindowOwner, FormatConstants.DetectingTitle, FormatConstants.DetectingDescription, FormatConstants.DetectingDescriptionExtra);

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
      multipleInputDataIndex = index;
      CloseMultipleInput += CloseMultipleInputDataView;
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
      var detectedOption = detectedOptions[index];
      diffController.CopyOptionValues(option, detectedOption);
      if (detectedOption.IsModifed)
      {
        MarkOptionChange((FormatOptionModel)option, true, FormatConstants.BoldFontWeight);
      }
      else
      {
        MarkOptionChange((FormatOptionModel)option, false, FormatConstants.NormalFontWeight);
      }
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

    private async Task ReloadDiffAsync(string title, string description, string descriptionExtra)
    {
      ShowDetectingView(diffWindow, title, description, descriptionExtra);
      diffWindow.IsEnabled = false;

      bool errorDetected = await AreOptionsValidAsync();
      if (errorDetected == false)
      {
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
        await Task.Delay(2000);
      }
      else
      {
        await Task.Delay(500);
      }

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

      diffWindow.DiffInput.Document = diffInput;
      diffWindow.DiffOutput.Document = diffOutput;
    }

    private void DetectionFinished(List<string> filesPath)
    {
      if (detectingView.IsLoaded)
      {
        InitializeDiffView(filesPath);

        infoWindow = new DetectedFormatStyleInfo(GetDetectedOptions());
        infoWindow.Closed += CloseInfoWindow;

        CloseDetectionView();

        diffWindow.Show();
        infoWindow.Owner = diffWindow;
        diffWindow.IsEnabled = false;

        infoWindow.Show();
      }
    }

    private void CloseInfoWindow(object sender, System.EventArgs e)
    {
      if (infoWindow == null)
        return;

      infoWindow.Closed -= CloseInfoWindow;
      infoWindow = null;
    }


    private string GetDetectedOptions()
    {
      var options = FormatOptionFile.CreateOutput(FormatOptions, SelectedStyle).ToString().Split(new string[] { "\r\n" }, StringSplitOptions.None);
      var sb = new StringBuilder();

      for (int i = 2; i < options.Length - 2; i++)
      {
        sb.AppendLine(options[i]);
      }
      return sb.ToString();
    }

    private void ShowDetectingView(DialogWindow detectingWindowOwner, string title, string description, string descriptionExtra)
    {
      detectingView = new DetectingView();
      detectingView.WindowTitle.Text = title;
      detectingView.Description.Text = description;
      detectingView.DescriptionExtra.Text = descriptionExtra;
      detectingView.Owner = detectingWindowOwner;

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

    private async Task<bool> AreOptionsValidAsync()
    {
      if (flowDocuments.Count == 0) return false;
      (var errorDetected, var errorMessage) = await diffController.CheckOptionValidityAsync(filesContent.First(), SelectedStyle, FormatOptions);

      if (errorDetected)
      {
        WriteErrorMessageInView(errorMessage);
      }
      return errorDetected;
    }

    private void WriteErrorMessageInView(string errorMessage)
    {
      foreach (var flowDocument in flowDocuments)
      {
        var paragraph = new Paragraph
        {
          Foreground = Brushes.Red,
          FontWeight = FontWeights.DemiBold
        };
        paragraph.Inlines.Add(errorMessage);

        flowDocument.Item2.Blocks.Clear();
        flowDocument.Item2.Blocks.Add(paragraph);
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

      if (infoWindow != null)
        infoWindow.Close();
    }

    private void CloseMultipleInputDataView(object sender, EventArgs e)
    {
      OptionChanged(multipleInputDataIndex);
      CloseMultipleInput -= CloseMultipleInputDataView;
    }

    private void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
  }
}