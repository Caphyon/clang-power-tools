using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.MVVM.Models;
using ClangPowerTools.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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

    #endregion


    #region Constructor

    public InputDataViewModel(string content)
    {
      CreateInputsCollection(content);
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
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("InputToAdd"));
      }
    }

    public ObservableCollection<InputDataModel> Inputs { get; set; } = new ObservableCollection<InputDataModel>();

    public ICommand AddCommand
    {
      get => addCommand ?? (addCommand = new RelayCommand(() => AddInput(), () => CanExecute));
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

      Inputs.RemoveAt(index);

      // Is not the last element update all the elements position from the index to the end
      if (index != Inputs.Count)
      {
        for (int position = index; position < Inputs.Count; ++position)
          Inputs[position].LineNumber = position + 1;
      }
    }

    private void AddInput()
    {
      if (string.IsNullOrWhiteSpace(inputToAdd))
        return;

      if (IsDuplicate(inputToAdd))
        return;

      AddNewElement(inputToAdd);
      InputToAdd = string.Empty;
    }

    private bool IsDuplicate(string element) => Inputs.FirstOrDefault(model => model.InputData == element) != null;

    private void CreateInputsCollection(string content)
    {
      if (string.IsNullOrWhiteSpace(content))
        return;

      var splitContent = content.Split(';').ToList();
      for (int index = 0; index < splitContent.Count; ++index)
        AddNewElement(splitContent[index]);
    }

    private void AddNewElement(string newElement)
    {
      var model = new InputDataModel(newElement);
      Inputs.Add(model);
    }

    #endregion
  }
}
