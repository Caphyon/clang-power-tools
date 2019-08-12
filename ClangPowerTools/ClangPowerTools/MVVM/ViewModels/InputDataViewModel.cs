using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.Views;
using System.ComponentModel;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class InputDataViewModel : INotifyPropertyChanged
  {
    #region Members
    public event PropertyChangedEventHandler PropertyChanged;

    private InputDataView inputDataView;
    private string textBoxInput = string.Empty;
    private ICommand okCommand;
    #endregion

    #region Constructor
    public InputDataViewModel(string content)
    {
      TextBoxInput = content;
    }
    #endregion

    #region Properties
    public string TextBoxInput
    {
      get
      {
        return textBoxInput;
      }
      set
      {
        textBoxInput = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TextBoxInput"));
      }
    }

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

   // TODO remove and use only view
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
