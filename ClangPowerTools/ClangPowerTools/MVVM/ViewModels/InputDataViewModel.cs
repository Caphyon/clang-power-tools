using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class InputDataViewModel : INotifyPropertyChanged
  {
    #region Members
    public event PropertyChangedEventHandler PropertyChanged;

    private InputDataView inputDataView;
    private ICommand okCommand;
    #endregion

    #region Constructor
    public InputDataViewModel(string content)
    {
      //TextBoxInput = content;
    }
    #endregion

    #region Properties
    public ObservableCollection<string> Inputs { get; set; } = new ObservableCollection<string>();

    public ICommand OkCommand
    {
      get => okCommand ?? (okCommand = new RelayCommand(() => ClickOKButton(), () => CanExecute));
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

    private void ClickOKButton()
    {
      inputDataView.Close();
    }
    #endregion
  }
}
