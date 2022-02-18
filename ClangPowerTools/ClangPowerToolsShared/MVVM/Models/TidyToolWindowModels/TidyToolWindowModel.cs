using ClangPowerToolsShared.MVVM.Commands;
using ClangPowerToolsShared.MVVM.Constants;
using System.ComponentModel;

namespace ClangPowerToolsShared.MVVM.Models.TidyToolWindowModels
{
  public class TidyToolWindowModel : INotifyPropertyChanged
  {
    #region Members

    public event PropertyChangedEventHandler PropertyChanged;
    private bool isRunning;
    private string progressBarVisibility;
    private string buttonVisibility;
    private bool isChecked;
    public bool isDiscardEnabled;
    public CountFilesModel CountFilesModel;

    #endregion

    #region Constructors

    public TidyToolWindowModel()
    {
      //init
      CountFilesModel = new CountFilesModel();
      RemoveIcon = new IconModel(IconResourceConstants.RemoveLight, UIElementsConstants.Visibile, true);
      DiscardFixIcon = new IconModel(IconResourceConstants.DiscardFixLight, UIElementsConstants.Visibile, true);
      TidyFixIcon = new IconModel(IconResourceConstants.FixLight, UIElementsConstants.Visibile, true);
      RefreshTidyIcon = new IconModel(IconResourceConstants.RemoveLight, UIElementsConstants.Visibile, true);

      ProgressBarVisibility = UIElementsConstants.Hidden;
      ButtonVisibility = UIElementsConstants.Visibile;
      EnableAllIcons();
    }

    #endregion

    #region Properties

    public IconModel RemoveIcon { get; set; }

    public IconModel DiscardFixIcon { get; set; }

    public IconModel TidyFixIcon { get; set; }

    public IconModel RefreshTidyIcon { get; set; }


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

    //public int TotalCheckedFiles
    //{
    //  get { return totalCheckedFiles; }
    //  set
    //  {
    //    totalCheckedFiles = value;
    //  }
    //}

    //public int TotalCheckedHeaders
    //{
    //  get { return totalCheckedHeaders; }
    //  set
    //  {
    //    totalCheckedHeaders = value;
    //  }
    //}

    //public int TotalCheckedFixedFiles
    //{
    //  get { return totalCheckedFixedFiles; }
    //  set
    //  {
    //    totalCheckedFixedFiles = value;
    //  }
    //}

    //public int TotalChecked
    //{
    //  get { return totalChecked; }
    //  set
    //  {
    //    totalChecked = value;
    //    //TidyTooltip = UIElementsConstants.RefreshTooltip + UIElementsConstants.TidyTooltip + TidyFilesNr.ToString() + UIElementsConstants.FilesTooltip;
    //    //RemoveTooltip = UIElementsConstants.IgnoreTooltip + totalChecked.ToString() + UIElementsConstants.FilesTooltip;
    //    //FixTooltip = UIElementsConstants.FixTooltip + FixFilesNr.ToString() + UIElementsConstants.FilesTooltip;
    //    //DiscardTooltip = UIElementsConstants.DiscardTooltip + totalChecked.ToString() + " fixed" + UIElementsConstants.FilesTooltip;
    //    //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TotalChecked"));
    //  }
    //}

    //public int DiscardNr
    //{
    //  get { return discardNr; }
    //  set
    //  {
    //    discardNr = value;
    //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DiscardNr"));
    //  }
    //}

    /// <summary>
    /// The next number of files will be fixed, except headers.
    /// Number will be displayed in tooltip
    /// </summary>
    //public bool IsEnabled
    //{
    //  get
    //  {
    //    if (!isRunning && totalChecked is not 0)
    //    {
    //      EnableAllIcons();
    //      if (TotalFixedChecked == 0)
    //        DisableDiscardFixIcon();
    //      if ((TotalChecked == TotalFixedChecked) || (TotalCheckedFiles == TotalCheckedFixedFiles))
    //        DisableFixIcon();
    //      if (TotalChecked == TotalCheckedHeaders)
    //        DisableTidyIcon();
    //      return true;
    //    }
    //    else
    //    {
    //      SelectDisableAllIcons();
    //    }
    //    return false;
    //  }
    //  set { }
    //}

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

    //public bool SelectedAll
    //{
    //  get
    //  {
    //    return selectedAll;
    //  }

    //  set
    //  {
    //    selectedAll = value;
    //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedAll"));
    //  }
    //}

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
      DiscardFixIcon.IconPath = IconResourceConstants.DiscardFixDisabled;
      DiscardFixIcon.IsEnabled = false;
    }

    private void DisableFixIcon()
    {
      TidyFixIcon.IconPath = IconResourceConstants.FixDisabled;
      TidyFixIcon.IsEnabled = false;
    }
    private void DisableTidyIcon()
    {
      RefreshTidyIcon.IconPath = IconResourceConstants.RefreshDisabled;
      RefreshTidyIcon.IsEnabled = false;
    }

    public void EnableAllIcons()
    {
      if (VSThemeCommand.GetCurrentVsTheme() == VsThemes.Dark)
        SelectAllIconsDarkTheme();
      else
        SelectAllIconsLightTheme();
    }

    public string ButtonVisibility
    {
      get { return buttonVisibility; }
      set
      {
        buttonVisibility = value;
        RefreshTidyIcon.Visibility = value;
        TidyFixIcon.Visibility = value;
        DiscardFixIcon.Visibility = value;
        RemoveIcon.Visibility = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ButtonVisibility"));
      }
    }

    #region Private Methods
    private void SelectAllIconsDarkTheme()
    {
      RemoveIcon.IconPath = IconResourceConstants.RemoveDark;
      DiscardFixIcon.IconPath = IconResourceConstants.DiscardFixDark;
      TidyFixIcon.IconPath = IconResourceConstants.FixDark;
      RefreshTidyIcon.IconPath = IconResourceConstants.RefreshTidyDark;
    }

    private void SelectAllIconsLightTheme()
    {
      RemoveIcon.IconPath = IconResourceConstants.RemoveLight;
      DiscardFixIcon.IconPath = IconResourceConstants.DiscardFixLight;
      TidyFixIcon.IconPath = IconResourceConstants.FixLight;
      RefreshTidyIcon.IconPath = IconResourceConstants.RefreshTidyLight;
    }

    private void SelectDisableAllIcons()
    {
      RemoveIcon.IconPath = IconResourceConstants.RemoveDisabled;
      DiscardFixIcon.IconPath = IconResourceConstants.DiscardFixDisabled;
      TidyFixIcon.IconPath = IconResourceConstants.FixDisabled;
      RefreshTidyIcon.IconPath = IconResourceConstants.RefreshDisabled;
    }

    #endregion

    #endregion
  }
}
