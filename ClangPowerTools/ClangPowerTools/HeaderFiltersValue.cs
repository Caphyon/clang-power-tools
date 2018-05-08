﻿using System;
using System.ComponentModel;

namespace ClangPowerTools
{
  public class HeaderFiltersValue : INotifyPropertyChanged
  {

    public HeaderFiltersValue(string aValue)
    {
      mHeaderFilterValue = aValue;
    }

    public String mHeaderFilterValue;

    public event PropertyChangedEventHandler PropertyChanged;

    public String HeaderFilters
    {
      get
      {
        return mHeaderFilterValue;
      }
      set
      {
        mHeaderFilterValue = value;
        if (PropertyChanged != null)
          PropertyChanged(this, new PropertyChangedEventArgs("HeaderFilters"));
      }
    }    
  }
}