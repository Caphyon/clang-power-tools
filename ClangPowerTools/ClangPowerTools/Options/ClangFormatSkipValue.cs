using System;
using System.ComponentModel;

namespace ClangPowerTools.Options
{
  public class ClangFormatSkipValue : INotifyPropertyChanged
  {
    #region Members 

    public event PropertyChangedEventHandler PropertyChanged;

    private String mValue;

    #endregion


    #region Constructor

    public ClangFormatSkipValue()
    {
      mValue = String.Empty;
    }

    public ClangFormatSkipValue(String aValue) => mValue = aValue;

    #endregion


    #region Properties

    public String Value
    {
      get
      {
        return mValue;
      }
      set
      {
        mValue = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value"));
      }
    }

    #endregion

  }
}
