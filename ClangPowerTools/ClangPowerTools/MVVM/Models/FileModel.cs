using ClangPowerTools.MVVM.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerTools.MVVM.Models
{
  class FileModel : INotifyPropertyChanged
  {
    private bool isChecked;

    public string FileName { get; set; }
    public bool IsChecked
    {
      get { return isChecked; }
      set
      {
        if (isChecked == value) return;
        isChecked = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsChecked"));
        EventBus.Notify("IsConvertButtonEnable");
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;
  }
}
