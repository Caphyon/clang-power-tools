using System.ComponentModel;

namespace ClangPowerTools.MVVM.Models
{
  class FileModel : INotifyPropertyChanged
  {
    public string FileName { get; set; }
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

    public event PropertyChangedEventHandler PropertyChanged;

    private bool isChecked;
  }
}
