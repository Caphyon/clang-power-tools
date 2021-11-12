using ClangPowerToolsShared.MVVM.Commands;
using ClangPowerToolsShared.MVVM.Constants;
using System.ComponentModel;

namespace ClangPowerTools.MVVM.Models
{
  public class FileModel : INotifyPropertyChanged
  {

    #region Members

    public event PropertyChangedEventHandler PropertyChanged;
    private bool isRunning;
    private bool isEnabled;
    private bool isFixed;
    private bool isChecked;
    private string diffVisibility;
    private string fixVisibility;
    private string fontStyle;

    //icons
    private string tidyFixIcon = string.Empty;
    private string diffIcon = string.Empty;

    #endregion

    #region Constructor

    public FileModel()
    {
      EnableIcon();
      DiffVisibility = UIElementsConstants.Hidden;
      FixVisibility = UIElementsConstants.Visibile;
      FontStyle = UIElementsConstants.NormalStyleFont;
      IsEnabled = true;
    }

    #endregion

    #region Public Properities

    public string FileName { get; set; }
    public string FullFileName { get; set; }
    public string CopyFullFileName { get; set; }

    public string DiffIcon
    {
      get { return diffIcon; }
      set
      {
        diffIcon = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DiffIcon"));
      }
    }

    public string TidyFixIcon
    {
      get { return tidyFixIcon; }
      set
      {
        tidyFixIcon = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TidyFixIcon"));
      }
    }

    public string FontStyle
    {
      get { return fontStyle; }
      set
      {
        fontStyle = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FontStyle"));
      }
    }

    public string DiffVisibility
    {
      get { return diffVisibility; }
      set
      {
        diffVisibility = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DiffVisibility"));
      }
    }

    public string FixVisibility
    {
      get { return fixVisibility; }
      set
      {
        fixVisibility = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FixVisibility"));
      }
    }

    public bool IsFixed
    {
      get
      {
        return isFixed;
      }
      set
      {
        if (value)
        {
          DiffVisibility = UIElementsConstants.Visibile;
          FixVisibility = UIElementsConstants.Hidden;
          FontStyle = UIElementsConstants.ObliqueStyleFont;
        }
        else
        {
          DiffVisibility = UIElementsConstants.Hidden;
          FixVisibility = UIElementsConstants.Visibile;
          FontStyle = UIElementsConstants.NormalStyleFont;
        }
        if (isFixed == value) return;
        isFixed = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsFixed"));
      }
    }

    public bool IsRunning
    {
      get
      {
        return isRunning;
      }

      set
      {
        if (value)
        {
          DisableIcon();
        }
        else
        {
          EnableIcon();
        }
        isRunning = value;
        IsEnabled = !value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsRunning"));
      }
    }

    public bool IsEnabled
    {
      get
      {
        return isEnabled;
      }
      set
      {
        isEnabled = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsEnabled"));
      }
    }

    public bool IsChecked
    {
      get { return isChecked; }
      set
      {
        if (isChecked == value) { return; }
        isChecked = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsChecked"));
      }
    }

    private void EnableIconDarkTheme()
    {
      TidyFixIcon = IconResourceConstants.FixDark;
      DiffIcon = IconResourceConstants.DiffDark;
    }


    private void EnableIconLightTheme()
    {
      TidyFixIcon = IconResourceConstants.FixLight;
      DiffIcon = IconResourceConstants.DiffLight;
    }

    private void DisableIcon()
    {
      TidyFixIcon = IconResourceConstants.FixDisabled;
      DiffIcon = IconResourceConstants.DiffDisabled;
    }

    public void EnableIcon()
    {
      if (VSThemeCommand.GetCurrentVsTheme() == VsThemes.Dark)
        EnableIconDarkTheme();
      else
        EnableIconLightTheme();
    }

    #endregion

  }
}
