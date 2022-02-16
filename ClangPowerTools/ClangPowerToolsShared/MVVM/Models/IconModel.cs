﻿using ClangPowerToolsShared.MVVM.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ClangPowerToolsShared.MVVM.Models
{
  public class IconModel : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    private string iconPath;
    private string visibility;
    private bool isEnabled;

    public string IconPath 
    { 
      get { return iconPath; }
      set
      {
        iconPath = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IconPath"));
      }
    }
    public string Visibility 
    {
      get { return visibility; }
      set
      {
        visibility = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Visibility"));
      }
    }
    public bool IsEnabled
    { 
      get { return isEnabled; }   
      set
      {
        isEnabled = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsEnabled"));
      }
    }

    public IconModel(string iconPath, string visibility = "Hidden", bool isEnabled = false)
    {
      this.iconPath = iconPath;
      this.visibility = visibility;
      this.isEnabled = isEnabled;
    }
  }
}
