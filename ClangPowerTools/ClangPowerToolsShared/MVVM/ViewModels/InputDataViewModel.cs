using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.MVVM.Models;
using ClangPowerTools.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class InputDataViewModel : CommonSettingsFunctionality, INotifyPropertyChanged
  {
    #region Members

    public event PropertyChangedEventHandler PropertyChanged;

    private string inputToAdd;
    private InputDataView inputDataView;
    private ICommand addCommand;
    private bool showFilePicker;
    private ICommand pickFilesCommand;
    private bool showFolderPicker;
    private ICommand pickFolderCommand;

    #endregion


    #region Constructor

    public InputDataViewModel(string content, bool showFilesPicker = false, bool showFolderPicker = false)
    {
      CreateInputsCollection(content);
      ShowFilesPicker = showFilesPicker;
      ShowFolderPicker = showFolderPicker;
    }

    public InputDataViewModel() { }

    #endregion


    #region Properties

    public string InputToAdd
    {
      get
      {
        return inputToAdd;
      }
      set
      {
        inputToAdd = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InputToAdd)));
      }
    }

    public ObservableCollection<InputDataModel> Inputs { get; set; } = new ObservableCollection<InputDataModel>();

    public bool ShowFilesPicker
    {
      get
      {
        return showFilePicker;
      }
      set
      {
        showFilePicker = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowFilesPicker)));
      }
    }

    public bool ShowFolderPicker
    {
      get
      {
        return showFolderPicker;
      }
      set
      {
        showFolderPicker = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowFolderPicker)));
      }
    }

    public ICommand AddCommand
    {
      get => addCommand ??= new RelayCommand(AddInput, () => CanExecute);
    }

    public bool CanExecute
    {
      get => true;
    }

    public ICommand PickFilesCommand
    {
      get => pickFilesCommand ??= new RelayCommand(PickFile, () => CanPickFilesExecute);
    }

    public bool CanPickFilesExecute
    {
      get => ShowFilesPicker;
    }

    public ICommand PickFolderCommand
    {
      get => pickFolderCommand ??= new RelayCommand(PickFolder, () => CanPickFolderExecute);
    }

    public bool CanPickFolderExecute
    {
      get => ShowFolderPicker;
    }

    #endregion


    #region Methods

    public void ShowViewDialog()
    {
      inputDataView = new InputDataView(this);
      inputDataView.ShowDialog();
    }

    private void PickFile()
    {
      var filesToAdd = OpenFiles(string.Empty, "*.h;*.cpp", "Header and Source files|*.h;*.cpp|Header files|*.h|Source files|*.cpp");
      if (filesToAdd == null)
        return; // no file selected
      foreach (var file in filesToAdd)
      {
        InputToAdd = file;
        AddInput(); // automatically add selected file
      }
    }

    private void PickFolder()
    {
      InputToAdd = BrowseForFolderFiles();
      AddInput(); // automatically add selected folder
    }

    public void DeleteInput(int index)
    {
      if (index < 0 || index >= Inputs.Count)
        return;

      Inputs.RemoveAt(index);
    }

    private void AddInput()
    {
      if (string.IsNullOrWhiteSpace(inputToAdd))
        return;

      if (IsDuplicate(inputToAdd))
      { 
        MessageBox.Show($"Ignored to add duplicate: {InputToAdd}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        InputToAdd = string.Empty;
        return;
      }

      if(!File.Exists(inputToAdd) && !Directory.Exists(inputToAdd))
      {
        MessageBox.Show($"The file or folder does not exist: {InputToAdd}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
      }

      if(File.Exists(inputToAdd))
      {
        DirectoryInfo filePath = new DirectoryInfo(inputToAdd);
        inputToAdd = filePath.Name;
      }

      AddNewElement(inputToAdd);
      InputToAdd = string.Empty;
    }

    private bool IsDuplicate(string element) => Inputs.FirstOrDefault(model => model.InputData == element) != null;

    private void CreateInputsCollection(string content)
    {
      if (string.IsNullOrWhiteSpace(content))
        return;

      var splitContent = content.Split(';').ToList();
      foreach (var splitItem in splitContent)
        AddNewElement(splitItem);
    }

    private void AddNewElement(string newElement)
    {
      var model = new InputDataModel(newElement);
      Inputs.Add(model);
    }

    #endregion
  }
}
