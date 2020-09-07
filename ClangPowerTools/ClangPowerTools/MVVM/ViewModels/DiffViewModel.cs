using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.MVVM.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class DiffViewModel
  {
    #region Members

    private readonly Action CreateFormatFile;
    private readonly DiffWindow diffWindow;
    private readonly List<(FlowDocument, FlowDocument)> flowDocuments;
    private ICommand createFormatFileCommand;
    private string selectedFile;
    private const int PageWith = 1000;

    #endregion


    #region Properties

    public string OptionsFile { get; set; }

    public List<string> Files { get; }

    public int SelectedIndex { get; set; }

    public string SelectedFile
    {
      get
      {
        if (string.IsNullOrEmpty(selectedFile) && Files.Count > 0)
        {
          selectedFile = Files.First();
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
    public DiffViewModel(DiffWindow diffWindow, List<(FlowDocument, FlowDocument)> flowDocuments, List<string> fileNames, string optionsFile, Action CreateFormatFile)
    {
      Files = fileNames;
      OptionsFile = optionsFile;
      this.CreateFormatFile = CreateFormatFile;
      this.diffWindow = diffWindow;
      this.flowDocuments = flowDocuments;

      SetFilesComboBoxVisibility();
      SetFlowDocuments();
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

    #region Private Methods

    private void SetFlowDocuments()
    {
      FlowDocument diffInput;
      FlowDocument diffOutput;
      if (Files.Count == 0)
      {
        diffInput = flowDocuments[0].Item1;
        diffOutput = flowDocuments[0].Item2;
      }
      else
      {
        if (string.IsNullOrEmpty(selectedFile))
        {
          SelectedFile = Files.First();
        }
        diffInput = flowDocuments[SelectedIndex].Item1;
        diffOutput = flowDocuments[SelectedIndex].Item2;

      }

      diffInput.PageWidth = PageWith;
      diffOutput.PageWidth = PageWith;
      diffWindow.DiffInput.Document = diffInput;
      diffWindow.DiffOutput.Document = diffOutput;
    }

    private void SetFilesComboBoxVisibility()
    {
      if (Files.Count == 0)
      {
        diffWindow.FilesComboBox.Visibility = Visibility.Hidden;
      }
      else
      {
        diffWindow.FilesComboBox.Visibility = Visibility.Visible;
      }
    }

    #endregion

  }
}
