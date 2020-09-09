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
  public class DiffViewModel : CommonSettingsFunctionality, INotifyPropertyChanged
  {
    #region Members

    public event PropertyChangedEventHandler PropertyChanged;

    private readonly DiffWindow diffWindow;
    private readonly DiffController diffController;
    private List<(FlowDocument, FlowDocument)> flowDocuments;
    private ICommand createFormatFileCommand;
    private string selectedFile;
    private const int PageWith = 1000;

    #endregion


    #region Properties

    public List<IFormatOption> FormatOptions { get; set; }
    public EditorStyles FormatStyle { get; set; }
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

    #endregion

    #region Private Methods

    public async Task DiffDocumentsAsync(List<string> filePaths)
    {
      var detectingView = new DetectingView();
      detectingView.Show();
      detectingView.Closed += diffController.CloseLoadingView;

      (FormatStyle, FormatOptions) = await diffController.GetFormatOptionsAsync(filePaths);
      flowDocuments = await diffController.CreateFlowDocumentsAsync(filePaths, FormatStyle, FormatOptions);

      if (detectingView.IsLoaded)
      {
        InitializeDiffView(filePaths);
        detectingView.Closed -= diffController.CloseLoadingView;
        detectingView.Close();
        diffWindow.Show();
      }
    }

    private void InitializeDiffView(List<string> filePaths)
    {
      FileNames = diffController.GetFileNames(filePaths);
      SetFlowDocuments();
      Style = "Style: " + FormatStyle.ToString();

      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FileNames"));
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FormatOptions"));
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Style"));
    }

    private void SetFlowDocuments()
    {
      FlowDocument diffInput;
      FlowDocument diffOutput;
      if (FileNames.Count == 0)
      {
        return;
      }

      //TODO need to test if section should be removed
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

    private void CreateFormatFile()
    {
      string fileName = ".clang-format";
      string defaultExt = ".clang-format";
      string filter = "Configuration files (.clang-format)|*.clang-format";

      string path = SaveFile(fileName, defaultExt, filter);
      if (string.IsNullOrEmpty(path) == false)
      {
        WriteContentToFile(path, FormatOptionFile.CreateOutput(FormatOptions, FormatStyle).ToString());
      }
    }

    #endregion

  }
}
