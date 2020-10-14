using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ClangPowerTools
{
  public class DetectedStyleInfoViewModel : INotifyPropertyChanged
  {
    #region Members

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion


    #region Constructor

    public DetectedStyleInfoViewModel(string styleInfo)
    {
      var styleInfoArray = styleInfo.Split('\n');
      foreach (var item in styleInfoArray)
        FlagsCollection.Add(item);
    }

    #endregion


    #region Properties

    public string DetectedOptions { get; set; }

    public ObservableCollection<string> FlagsCollection { get; set; } = new ObservableCollection<string>();


    #endregion

    #region Private Methods

    private void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
  }
}
