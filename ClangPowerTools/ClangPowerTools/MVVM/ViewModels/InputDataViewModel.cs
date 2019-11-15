using ClangPowerTools.MVVM.Commands;
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

    private string input;
    private InputDataView inputDataView;
    private ICommand addCommand;
    #endregion

    #region Constructor
    public InputDataViewModel(string content)
    {
      Inputs = new ObservableCollection<string>(content.Split(';').ToList());
    }

    public InputDataViewModel() { }
    #endregion

    #region Properties
    public string Input
    {
      get
      {
        return input;
      }
      set
      {
        input = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Input"));
      }
    }

    public ObservableCollection<string> Inputs { get; set; }

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
      if (string.IsNullOrEmpty(input) == false)
        Inputs.Add(input);
    }
    #endregion
  }
}
