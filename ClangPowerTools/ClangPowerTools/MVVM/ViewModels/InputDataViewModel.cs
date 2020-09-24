using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.MVVM.Models;
using ClangPowerTools.MVVM.Views;
using ClangPowerTools.Views;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace ClangPowerTools
{
  public class InputDataViewModel : CommonSettingsFunctionality, INotifyPropertyChanged
  {
    #region Members

    public event PropertyChangedEventHandler PropertyChanged;

    private string inputToAdd;
    private string placeholder;
    private InputDataView inputDataView;
    private ICommand addCommand;
    private ICommand clearCommand;
    private ICommand detectFormatStyleCommand;
    private string warningText;
    private readonly DetectFormatStyleMenuView view;

    private const string MAX_FILE_WARNING = "This action will take some time due to the number of selected files.";

    private const long MAX_FILE_SIZE = 1500000;
    private long totalFilesSize = 0;

    #endregion


    #region Constructor

    public InputDataViewModel(string content)
    {
      CreateInputsCollection(content);
    }

    public InputDataViewModel() { }

    public InputDataViewModel(DetectFormatStyleMenuView view, bool browse)
    {
      this.view = view;
      BrowseForFiles = browse;
      view.InputList.ClearButton.IsEnabled = false;
      view.DetectFormatStyleButton.IsEnabled = false;
    }

    #endregion


    #region Properties

    private bool BrowseForFiles { get; set; } = false;

    public string InputToAdd
    {
      get
      {
        return inputToAdd;
      }
      set
      {
        inputToAdd = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("InputToAdd"));
      }
    }

    public string Placeholder
    {
      get
      {
        return placeholder;
      }
      set
      {
        placeholder = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Placeholder"));
      }
    }

    public string WarningText
    {
      get
      {
        return warningText;
      }
      set
      {
        warningText = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("WarningText"));
      }
    }

    public ObservableCollection<InputDataModel> Inputs { get; set; } = new ObservableCollection<InputDataModel>();

    public ICommand AddCommand
    {
      get => addCommand ?? (addCommand = new RelayCommand(() => AddInput(), () => CanExecute));
    }
    public ICommand ClearCommand
    {
      get => clearCommand ?? (clearCommand = new RelayCommand(() => ClearList(), () => CanExecute));
    }

    public ICommand DetectFormatStyleCommand
    {
      get => detectFormatStyleCommand ?? (detectFormatStyleCommand = new RelayCommand(() => DetectFormatStyleAsync().SafeFireAndForget(), () => CanExecute));
    }

    public bool CanExecute
    {
      get
      {
        return true;
      }
    }

    #endregion


    #region Methods

    public void ShowViewDialog()
    {
      inputDataView = new InputDataView(this);
      inputDataView.ShowDialog();
    }

    public void DeleteInput(int index)
    {
      if (index < 0 || index >= Inputs.Count)
        return;

      var model = Inputs[index];
      totalFilesSize -= model.FileSize;

      Inputs.RemoveAt(index);

      // Is not the last element update all the elements position from the index to the end
      if (index != Inputs.Count)
      {
        for (int position = index; position < Inputs.Count; ++position)
          Inputs[position].LineNumber = position + 1;
      }

      if (Inputs.Count == 0)
      {
        view.InputList.ClearButton.IsEnabled = false;
        view.DetectFormatStyleButton.IsEnabled = false;
      }

      if (view == null)
        return;

      if (totalFilesSize <= MAX_FILE_SIZE)
        view.WarningTextBox.Visibility = System.Windows.Visibility.Hidden;
    }

    private void AddInput()
    {
      if (BrowseForFiles && string.IsNullOrWhiteSpace(inputToAdd))
        AddBrowseFilePathsToCollection();
      else
        AddInputToCollection();

      if (Inputs.Count > 0)
      {
        view.InputList.ClearButton.IsEnabled = true;
        view.DetectFormatStyleButton.IsEnabled = true;
      }

      if (view == null)
        return;

      if (totalFilesSize > MAX_FILE_SIZE)
      {
        WarningText = MAX_FILE_WARNING;
        view.WarningTextBox.Foreground = Brushes.Orange;
        view.WarningTextBox.Visibility = System.Windows.Visibility.Visible;

        var warning = new FileSizeWarningView(view);
        warning.ShowDialog();
      }
    }

    private void AddBrowseFilePathsToCollection()
    {
      var filePaths = OpenFiles(string.Empty, ".cpp", ScriptConstants.FileExtensionsSelectFile);

      if (filePaths == null || filePaths.Length <= 0)
        return;

      for (int index = 0; index < filePaths.Length; ++index)
      {
        if (IsDuplicate(filePaths[index]))
          continue;

        int position = Inputs.Count == 0 ? 1 : Inputs.Last().LineNumber + 1;
        AddNewElement(filePaths[index], position);
      }
    }

    private void AddInputToCollection()
    {
      if (string.IsNullOrWhiteSpace(inputToAdd))
        return;

      if (IsDuplicate(inputToAdd))
        return;

      int index = Inputs.Count == 0 ? 1 : Inputs.Last().LineNumber + 1;
      AddNewElement(inputToAdd, index);
      InputToAdd = string.Empty;
    }

    private bool IsDuplicate(string filePath) => Inputs.Where(model => model.InputData == filePath).Count() > 0;

    private void CreateInputsCollection(string content)
    {
      if (string.IsNullOrWhiteSpace(content))
        return;

      var splitContent = content.Split(';').ToList();
      for (int index = 0; index < splitContent.Count; ++index)
        AddNewElement(splitContent[index], index + 1);
    }

    private void AddNewElement(string filePath, int index)
    {
      var model = new InputDataModel(filePath, index);
      totalFilesSize += model.FileSize;
      Inputs.Add(model);
    }

    private void ClearList()
    {
      Inputs.Clear();

      view.InputList.ClearButton.IsEnabled = false;
      view.DetectFormatStyleButton.IsEnabled = false;

      if (view != null)
        view.WarningTextBox.Visibility = System.Windows.Visibility.Hidden;
    }

    private async Task DetectFormatStyleAsync()
    {
      var diffWin = new DiffWindow();
      List<string> filesPath = Inputs.Select(model => model.InputData).ToList();

      view.IsEnabled = false;
      await diffWin.ShowDiffAsync(filesPath, view);
      view.IsEnabled = true;
    }

    #endregion
  }
}
