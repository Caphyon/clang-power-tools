using ClangPowerTools.MVVM.Utils;
using ClangPowerTools.Properties;
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

    public static int numberOfCheckedFiles { get; set; } = 0;
    public string FileName { get; set; }
    public bool IsChecked
    {
      get { return isChecked; }
      set
      {
        if (isChecked == value) { return; }
        isChecked = value;
        if (value)
        {
          numberOfCheckedFiles++;
        }
        else
        {
          numberOfCheckedFiles--;
        }
        EventBus.Notify(numberOfCheckedFiles == 0 ? "DisableConvertButtonEvent" : "EnableConvertButtonEvent");
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsChecked"));
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;
  }
}
