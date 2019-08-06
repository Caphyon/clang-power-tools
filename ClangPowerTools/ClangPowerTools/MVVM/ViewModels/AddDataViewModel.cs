using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.Views;
using System.ComponentModel;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class AddDataViewModel : INotifyPropertyChanged
  {
    #region Members
    public event PropertyChangedEventHandler PropertyChanged;

    private AddDataView addDataView;
    private string textBoxInput = string.Empty;
    private ICommand okCommand;
    #endregion

    #region Constructor
    public AddDataViewModel(string content)
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

    #region Methods
    public void ShowViewDialog()
    {
      addDataView = new AddDataView(this);
      addDataView.ShowDialog();
    }

    private void ClickOKButton()
    {
      addDataView.Close();
    }
    #endregion
  }
}
