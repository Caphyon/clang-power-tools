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

    private ObservableCollection<InputDataModel> inputs = new ObservableCollection<InputDataModel>();
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

    public ObservableCollection<InputDataModel> Inputs
    {
      get { return inputs }
      set { inputs = value; }
    }


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
        inputs.RemoveAt(index);
    }

    private void AddInput()
    {
      if (string.IsNullOrEmpty(inputToAdd) == false)
        inputs.Add(new InputDataModel(inputToAdd));
    }

    private void CreateInputsCollection(string content)
    {
      var splitContent = content.Split(';').ToList();
      foreach (var item in splitContent)
      {
        inputs.Add(new InputDataModel(item));
      }
    }

    #endregion
  }
}
