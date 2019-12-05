using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.MVVM.Models;
using ClangPowerTools.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class InputDataViewModel : INotifyPropertyChanged
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
      if (index >= 0)
        Inputs.RemoveAt(index);
    }

    private void AddInput()
    {
      if (string.IsNullOrWhiteSpace(inputToAdd) == false)
      {
        Inputs.Add(new InputDataModel(inputToAdd));
        InputToAdd = string.Empty;
      }
    }

    private void CreateInputsCollection(string content)
    {
      if (string.IsNullOrWhiteSpace(content)) return;

      var splitContent = content.Split(';').ToList();
      foreach (var item in splitContent)
      {
        Inputs.Add(new InputDataModel(item));
      }
    }

    #endregion
  }
}
