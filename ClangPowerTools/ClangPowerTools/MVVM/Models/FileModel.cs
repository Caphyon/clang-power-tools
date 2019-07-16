using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerTools.MVVM.Models
{
  class FileModel: INotifyPropertyChanged
  {
    private bool _isChecked;
    public string FileName { get; set; }
    public bool IsChecked
    {
      get { return _isChecked; }
      set
      {
        if (_isChecked == value) return;
        _isChecked = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsChecked"));
      }
    }


    public event PropertyChangedEventHandler PropertyChanged;
  }
}
