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


    #region Constructor 

    public InputMultipleDataViewModel(string input)
    {
      this.input = input;
    }

    #endregion

    #region Properties

    public string Input
    {
      get => input;
      set => input = value;
    }

    #endregion



  }
}
