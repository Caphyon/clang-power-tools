using System;
using System.ComponentModel;

namespace ClangPowerTools
{
  public class HeaderFiltersValue
  {
    #region Members

    private string mHeaderFilterValue;

    #endregion


    #region Constructor

    public HeaderFiltersValue(string aValue)
    {
      mHeaderFilterValue = aValue;
    }

    #endregion


    #region Properties

    public string HeaderFilters
    {
      get
      {
        return mHeaderFilterValue;
      }
      set
      {
        mHeaderFilterValue = value;
      }
    }
    
    #endregion

  }
}