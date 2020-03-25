using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerTools
{ 
  public class InputMultipleDataViewModel :INotifyPropertyChanged
  {
    #region Members

    public event PropertyChangedEventHandler PropertyChanged;

    private string input;

    #endregion


    #region Properties

    public string Input
    {
      get
      {
        return input;
      }
      set
      {
        input = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Input"));
      }
    }

    #endregion



  }
}
