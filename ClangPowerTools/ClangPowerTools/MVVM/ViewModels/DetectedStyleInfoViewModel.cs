using System.ComponentModel;

namespace ClangPowerTools
{
  public class DetectedStyleInfoViewModel : INotifyPropertyChanged
  {
    #region Members

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion


    #region Properties

    public string DetectedOptions { get; set; }

    #endregion

    #region Private Methods

    private void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
  }
}
