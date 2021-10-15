using System.ComponentModel;

namespace ClangPowerTools.MVVM.Models
{
  public class FileModel : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    #region Public Members

    public string FileName { get; set; }
    public string FullFileName { get; set; }
    public bool IsFixed
    { 
      get { return isFixed; } 
      set 
      { 
        if(isFixed == value) return;
        isFixed = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsFixed"));
      }
    }
    public bool IsChecked
    {
      get { return isChecked; }
      set
      {
        if (isChecked == value) { return; }
        isChecked = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsChecked"));
      }
    }

    #endregion

    #region Private Members

    private bool isFixed;
    private bool isChecked;

    #endregion 

  }
}
