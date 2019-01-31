using System;
using System.ComponentModel;

namespace ClangPowerTools.Options
{
  public class ClangFormatPathValue
  {
    #region Members 

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
      }
    }

    #endregion

  }
}
