using ClangPowerTools.MVVM;
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

    public bool IsChecked
    {
      get
      {
        return isChecked;
      }
      set
      {
        // Take in consideration every change of the state if the collection is not empty
        if (!CollectionElementsCounter.IsEmpty())
        {
          if (value)
            CollectionElementsCounter.Add();
          else
            CollectionElementsCounter.Remove();
        }

        isChecked = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsChecked"));
      }
    }

    #endregion

  }
}
