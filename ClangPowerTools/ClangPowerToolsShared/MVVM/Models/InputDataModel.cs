using System.ComponentModel;

namespace ClangPowerTools.MVVM.Models
{
  public class InputDataModel : INotifyPropertyChanged
  {
    #region Members

    public event PropertyChangedEventHandler PropertyChanged;

    private bool isReadOnly = false;
    private string inputData = string.Empty;

    #endregion

    public InputDataModel(string input)
    {
      inputData = input;
    }

    #region Properties

    public string InputData
    {
      get
      {
        return inputData;
      }
      set
      {
        inputData = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("InputData"));
      }
    }

    public bool IsReadOnly
    {
      get
      {
        return isReadOnly;
      }
      set
      {
        isReadOnly = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CanEdit"));
      }
    }

    #endregion
  }
}
