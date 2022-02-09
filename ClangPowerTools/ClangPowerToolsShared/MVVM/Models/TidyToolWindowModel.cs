using ClangPowerTools.Helpers;
using ClangPowerTools.Services;
using ClangPowerToolsShared.MVVM.Commands;
using ClangPowerToolsShared.MVVM.Constants;
using EnvDTE80;
using System;
using System.ComponentModel;
using System.IO;

namespace ClangPowerToolsShared.MVVM.Models
{
  public class TidyToolWindowModel
  {
    #region Members

    public event PropertyChangedEventHandler PropertyChanged;

    private bool isRunning;
    private bool selectedAll;
    private string progressBarVisibility;
    private string buttonVisibility;
    private int tidyNr = 0;
    private int fixedNr = 0;
    private int fixFileNr = 0;
    private int removeNr = 0;
    private int discardNr = 0;
    private string removeTooltip = string.Empty;
    private string discardTooltip = string.Empty;
    private string tidyTooltip = string.Empty;
    private string fixTooltip = string.Empty;
    private int totalChecked = 0;
    private bool isChecked;
    public bool isDiscardEnabled;

    //icons
    private string removeIcon = string.Empty;
    private string discardFixIcon = string.Empty;
    private string tidyFixIcon = string.Empty;
    private string refreshTidyIcon = string.Empty;
    private string diffIcon = string.Empty;

    #endregion

    #region Constructors

    public TidyToolWindowModel()
    {
      EnableAllIcons();
      ProgressBarVisibility = UIElementsConstants.Hidden;
      ButtonVisibility = UIElementsConstants.Visibile;
    }

    #endregion

    #region Properties

    public string RemoveIcon
    {
      get { return removeIcon; }
      set
      {
        removeIcon = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RemoveIcon"));
      }
    }

    public int TotalFixedChecked { get; set; }

    public string DiscardFixIcon
    {
      get { return discardFixIcon; }
      set
      {
        discardFixIcon = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DiscardFixIcon"));
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

    public string RefreshTidyIcon
    {
      get { return refreshTidyIcon; }
      set
      {
        refreshTidyIcon = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RefreshTidyIcon"));
      }
    }

    public string RemoveTooltip
    {
      get { return removeTooltip; }
      set
      {
        removeTooltip = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RemoveTooltip"));
      }
    }

    public string DiscardTooltip
    {
      get { return discardTooltip; }
      set
      {
        discardTooltip = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DiscardTooltip"));
      }
    }

    public string FixTooltip
    {
      get { return fixTooltip; }
      set
      {
        fixTooltip = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FixTooltip"));
      }
    }

    public string TidyTooltip
    {
      get { return tidyTooltip; }
      set
      {
        tidyTooltip = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TidyTooltip"));
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

    public int TotalChecked
    {
      get { return totalChecked; }
      set
      {
        totalChecked = value;
        TidyTooltip = UIElementsConstants.RefreshTooltip + UIElementsConstants.TidyTooltip + totalChecked.ToString() + UIElementsConstants.FilesTooltip;
        RemoveTooltip = UIElementsConstants.IgnoreTooltip + totalChecked.ToString() + UIElementsConstants.FilesTooltip;
        FixTooltip = UIElementsConstants.FixTooltip + FixFilesNr.ToString() + UIElementsConstants.FilesTooltip;
        DiscardTooltip = UIElementsConstants.DiscardTooltip + totalChecked.ToString() + " fixed" + UIElementsConstants.FilesTooltip;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TotalChecked"));
      }
    }

    public int DiscardNr
    {
      get { return discardNr; }
      set
      {
        discardNr = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DiscardNr"));
      }
    }

    public int RemoveNr
    {
      get { return removeNr; }
      set
      {
        removeNr = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RemoveNr"));
      }
    }

    public int FixedNr
    {
      get { return fixedNr; }
      set
      {
        fixedNr = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FixedNr"));
      }
    }

    /// <summary>
    /// The next number of files will be fixed, except headers.
    /// Number will be displayed in tooltip
    /// </summary>
    public int FixFilesNr
    {
      get { return fixFileNr; }
      set
      {
        fixFileNr = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FixFilesNr"));
      }
    }

    public int TidyNr
    {
      get { return tidyNr; }
      set
      {
        tidyNr = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TidyNr"));
      }
    }

    public bool IsEnabled
    {
      get
      {
        if (!isRunning && totalChecked is not 0)
        {
          EnableAllIcons();
          if (TotalFixedChecked == 0)
            DisableDiscardFixIcon();
          if (TotalChecked == TotalFixedChecked)
            DisableFixIcon();
          return true;
        }
        else
        {
          DisableAllIcons();
        }
        return false;
      }
      set { }
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
          ProgressBarVisibility = UIElementsConstants.Visibile;
          ButtonVisibility = UIElementsConstants.Hidden;
        }
        else
        {
          ProgressBarVisibility = UIElementsConstants.Hidden;
          ButtonVisibility = UIElementsConstants.Visibile;
        }
        isRunning = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsRunning"));
      }
    }

    public bool SelectedAll
    {
      get
      {
        return selectedAll;
      }

      set
      {
        selectedAll = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedAll"));
      }
    }

    public string ProgressBarVisibility
    {
      get { return progressBarVisibility; }
      set
      {
        progressBarVisibility = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ProgressBarVisibility"));
      }
    }

    public void DisableDiscardFixIcon()
    {
      DiscardFixIcon = IconResourceConstants.RemoveFixDisabled;
    }

    public void DisableFixIcon()
    {
      TidyFixIcon = IconResourceConstants.FixDisabled;
    }

    public void EnableAllIcons()
    {
      if (VSThemeCommand.GetCurrentVsTheme() == VsThemes.Dark)
        EnableAllIconsDarkTheme();
      else
        EnableAllIconsLightTheme();
    }

    public string ButtonVisibility
    {
      get { return buttonVisibility; }
      set
      {
        buttonVisibility = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ButtonVisibility"));
      }
    }

    #region Private Methods
    private void EnableAllIconsDarkTheme()
    {
      RemoveIcon = IconResourceConstants.RemoveDark;
      DiscardFixIcon = IconResourceConstants.RemoveFixDark;
      TidyFixIcon = IconResourceConstants.FixDark;
      RefreshTidyIcon = IconResourceConstants.RefreshTidyDark;
    }


    private void EnableAllIconsLightTheme()
    {
      RemoveIcon = IconResourceConstants.RemoveLight;
      DiscardFixIcon = IconResourceConstants.RemoveFixLight;
      TidyFixIcon = IconResourceConstants.FixLight;
      RefreshTidyIcon = IconResourceConstants.RefreshTidyLight;
    }

    private void DisableAllIcons()
    {
      RemoveIcon = IconResourceConstants.RemoveDisabled;
      DiscardFixIcon = IconResourceConstants.RemoveFixDisabled;
      TidyFixIcon = IconResourceConstants.FixDisabled;
      RefreshTidyIcon = IconResourceConstants.RefreshDisabled;
    }

    #endregion

    #endregion
  }
}
