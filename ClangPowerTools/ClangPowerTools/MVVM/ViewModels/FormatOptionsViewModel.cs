using ClangPowerTools.MVVM.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;

namespace ClangPowerTools
{
  public class FormatOptionsViewModel : CommonSettingsFunctionality, INotifyPropertyChanged
  {
    #region Members

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion

    #region Constructor

    public FormatOptionsViewModel()
    {

    }

    #endregion

    #region Properties

    public List<IFormatOption> ToggleFormatOptions
    {
      get
      {
        return FormatOptions.FormatOptionsList;
      }
    }

 

    #endregion
  }
}
