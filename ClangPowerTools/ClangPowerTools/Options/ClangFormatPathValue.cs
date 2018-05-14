using System;
using System.ComponentModel;

namespace ClangPowerTools.Options
{
  public class ClangFormatPathValue : INotifyPropertyChanged
  {
    #region Members 

    public event PropertyChangedEventHandler PropertyChanged;

    private String mPath;

    #endregion

    #region Properties

    public bool Enable { get; set; } = false;

    public string Path
    {
      get
      {
        return mPath;
      }
      set
      {
        mPath = value;
        if (PropertyChanged != null)
          PropertyChanged(this, new PropertyChangedEventArgs("Path"));
      }
    }

    #endregion

  }
}
