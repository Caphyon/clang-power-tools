using System.ComponentModel;

namespace ClangPowerTools
{
  public class TidyCheckModel : INotifyPropertyChanged
  {
    #region Members

    public event PropertyChangedEventHandler PropertyChanged;
    private bool isChecked = false;

    #endregion


    #region Properties

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsChecked
    {
      get
      {
        return isChecked;
      }
      set
      {
        isChecked = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsChecked"));
      }
    }

    #endregion

  }
}
