using ClangPowerToolsShared.MVVM.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ClangPowerToolsShared.MVVM.Models
{
  public class IconModel : INotifyPropertyChanged
  {
    #region Members

    private string iconPath;
    private string visibility;
    private string tooltip;
    private bool isEnabled;

    #endregion

    #region Constructors

    public IconModel()
    {

    }

    public IconModel(string iconPath, string visibility, bool isEnabled, string tooltip = "")
    {
      this.iconPath = iconPath;
      this.visibility = visibility;
      this.isEnabled = isEnabled;
      this.tooltip = tooltip;
    }

    #endregion

    #region Properties

    public event PropertyChangedEventHandler PropertyChanged;

    public string IconPath 
    { 
      get { return iconPath; }
      set
      {
        iconPath = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IconPath"));
      }
    }

    public string Tooltip
    {
      get { return tooltip; }
      set
      {
        tooltip = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Tooltip"));
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

    #endregion
  }
}
