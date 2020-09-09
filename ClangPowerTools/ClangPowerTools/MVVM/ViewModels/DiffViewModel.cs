using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.MVVM.Controllers;
using ClangPowerTools.MVVM.Interfaces;
using ClangPowerTools.MVVM.Views;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class DiffViewModel : INotifyPropertyChanged
  {
    #region Members

    public event PropertyChangedEventHandler PropertyChanged;

    private readonly DiffWindow diffWindow;
    private readonly DiffController diffController;
    private List<(FlowDocument, FlowDocument)> flowDocuments;
    private List<string> fileNames;
    private ICommand createFormatFileCommand;
    private string selectedFile;
    private const int PageWith = 1000;

    #endregion


    #region Properties

    public List<IFormatOption> FormatOptions { get; set; }
    public EditorStyles FormatStyle { get; set; }

    public string OptionsFile { get; set; }

    public List<string> FileNames
    {
      get
      {
        return fileNames;
      }
      set
      {
        fileNames = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FileNames"));
      }
    }

    public int SelectedIndex { get; set; }

    public string SelectedFile
    {
      get
      {
        if (string.IsNullOrEmpty(selectedFile) && FileNames.Count > 0)
        {
          selectedFile = fileNames.First();
        }
        if (diffWindow.IsActive)
        {
          SetFlowDocuments();
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
    public DiffViewModel(DiffWindow diffWindow)
    {
      this.diffWindow = diffWindow;
      diffController = new DiffController();
      fileNames = new List<string>();
    }

    //Empty constructor used for XAML IntelliSense
    public DiffViewModel()
    {

    }

    #endregion

    #region Commands

    public ICommand CreateFormatFileCommand
    {
      // TODO change method to export
      get => createFormatFileCommand ??= new RelayCommand(() => SetFlowDocuments(), () => CanExecute);
    }

    #endregion

    #region Private Methods

    public async Task DiffDocumentsAsync(List<string> filePaths)
    {
      var detectingView = new DetectingView();
      //{
      //  Owner = diffWindow
      //};
      detectingView.Show();
      detectingView.Closed += diffController.CloseLoadingView;

      (FormatStyle, FormatOptions) = await diffController.GetFormatOptionsAsync(filePaths);
      flowDocuments = await diffController.CreateFlowDocumentsAsync(filePaths, FormatStyle, FormatOptions);
      FileNames = diffController.GetFileNames(filePaths);
      SetFlowDocuments();

      // TODO remove
      OptionsFile = string.Empty;


      //if (detectingView.IsLoaded == false)
      //{
      //  formatEditorView.IsEnabled = true;
      //  return;
      //}

      //SetEditorStyleOptions(matchedStyle, matchedOptions);


      detectingView.Closed -= diffController.CloseLoadingView;
      detectingView.Close();

      diffWindow.Show();
    }

    private void SetFlowDocuments()
    {
      FlowDocument diffInput;
      FlowDocument diffOutput;
      if (FileNames.Count == 0)
      {
        return;
      }

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

    #endregion

  }
}
