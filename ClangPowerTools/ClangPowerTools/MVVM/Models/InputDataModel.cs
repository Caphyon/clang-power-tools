using System.ComponentModel;
using System.IO;

namespace ClangPowerTools.MVVM.Models
{
  public class InputDataModel : INotifyPropertyChanged
  {
    #region Members

    public event PropertyChangedEventHandler PropertyChanged;

    private bool isReadOnly = false;
    private int lineNumber = 0;
    private string inputData = string.Empty;

    #endregion

    public InputDataModel(string input)
    {
      inputData = input;
    }

    public InputDataModel(string input, int line)
    {
      inputData = input;
      if (File.Exists(inputData))
      {
        var fileInfo = new FileInfo(inputData);
        FileSize = fileInfo.Length;
      }

      lineNumber = line;
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

    public int LineNumber
    {
      get
      {
        return lineNumber;
      }
      set
      {
        lineNumber = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LineNumber"));
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

    public long FileSize { get; private set; } = 0;

    #endregion
  }
}
