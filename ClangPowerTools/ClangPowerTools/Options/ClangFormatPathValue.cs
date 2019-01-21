using System;
using System.ComponentModel;

namespace ClangPowerTools.Options
{
  public class ClangFormatPathValue : INotifyPropertyChanged
  {
    #region Members 

    public event PropertyChangedEventHandler PropertyChanged;

    private string mValue = string.Empty;

    #endregion


    #region Properties

    public bool Enable { get; set; } = false;

    public string Value
    {
      get
      {
        return mValue;
      }
      set
      {
        mValue = value;
        if (PropertyChanged != null)
          PropertyChanged(this, new PropertyChangedEventArgs("Value"));
      }
    }

    #endregion

  }
}
