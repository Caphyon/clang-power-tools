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
      discardFixIcon = new IconModel(IconResourceConstants.DiscardFixDisabled, UIElementsConstants.Visibile, false);
      tidyFixIcon = new IconModel(VSThemeCommand.GetTidyFixIconEnabled(), UIElementsConstants.Visibile, true);
      refreshTidyIcon = new IconModel(VSThemeCommand.GetRefreshTidyIconEnabled(), UIElementsConstants.Visibile, true);
      removeIcon = new IconModel(VSThemeCommand.GetIgnoreIconEnabled(), UIElementsConstants.Visibile, true);

      //Init
      CountFilesModel = new CountFilesModel();

      //Register events
      CountFilesModel.PropertyChanged += UpdateIconsOnPropertyChange;

      //init private icons
      discardFixIcon = new IconModel(IconResourceConstants.DiscardFixDisabled, UIElementsConstants.Visibile, false);

      //Init public icons
      RemoveIcon = new IconModel(VSThemeCommand.GetIgnoreIconEnabled(), UIElementsConstants.Visibile, true);
      DiscardFixIcon = new IconModel(IconResourceConstants.DiscardFixDisabled, UIElementsConstants.Visibile, false);
      TidyFixIcon = new IconModel(VSThemeCommand.GetTidyFixIconEnabled(), UIElementsConstants.Visibile, true);
      RefreshTidyIcon = new IconModel(VSThemeCommand.GetRefreshTidyIconEnabled(), UIElementsConstants.Visibile, true);
      
      //Hide progress bar
      ProgressBarVisibility = UIElementsConstants.Hidden;
      ButtonVisibility = UIElementsConstants.Visibile;
    }

    #endregion

    #region Properties


    /// <summary>
    /// Update icons
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void UpdateIconsOnPropertyChange(object sender, System.EventArgs e)
    {
      TidyFixIcon = tidyFixIcon;
      DiscardFixIcon = discardFixIcon;
      RefreshTidyIcon = refreshTidyIcon;
      RemoveIcon = removeIcon;
    }
    private IconModel discardFixIcon;
    public IconModel DiscardFixIcon
    {
      get
      {
        return discardFixIcon;
      }
      set
      {
        if (CountFilesModel.TotalCheckedFixedFiles == 0)
        {
          discardFixIcon.IconPath = IconResourceConstants.DiscardFixDisabled;
          discardFixIcon.IsEnabled = false;
        }
        else
        {
          discardFixIcon.IconPath = VSThemeCommand.GetDiscardFixIconEnabled();
          discardFixIcon.IsEnabled = true;
        }
        discardFixIcon = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DiscardFixIcon"));
      }
    }

    private IconModel tidyFixIcon;
    public IconModel TidyFixIcon 
    {
      get { return tidyFixIcon; }
      set
      {
        if (CountFilesModel.TotalCheckedFiles == 0 ||
          CountFilesModel.TotalCheckedSourceFiles == CountFilesModel.TotalCheckedFixedSouceFiles ||
          CountFilesModel.TotalCheckedSourceFiles == 0)
        {
          tidyFixIcon.IconPath = IconResourceConstants.FixDisabled;
          tidyFixIcon.IsEnabled = false;
        }
        else
        {
          tidyFixIcon.IconPath = VSThemeCommand.GetTidyFixIconEnabled();
          tidyFixIcon.IsEnabled = true;
        }
        tidyFixIcon = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TidyFixIcon"));
      }
    }

    public IconModel refreshTidyIcon;
    public IconModel RefreshTidyIcon {
      get { return refreshTidyIcon; }
      set
      {
        if(CountFilesModel.TotalCheckedFiles == 0 ||
          CountFilesModel.TotalCheckedHeaders == CountFilesModel.TotalCheckedFiles)
        {
          refreshTidyIcon.IconPath = IconResourceConstants.RefreshDisabled;
          refreshTidyIcon.IsEnabled = false;
        }
        else
        {
          refreshTidyIcon.IconPath = VSThemeCommand.GetRefreshTidyIconEnabled();
          refreshTidyIcon.IsEnabled = true;
        }

        refreshTidyIcon = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RefreshTidyIcon"));
      }
    }
    public IconModel removeIcon { get; set; }
    public IconModel RemoveIcon
    {
      get { return removeIcon; }
      set
      {
        if(CountFilesModel.TotalCheckedFiles == 0)
        {
          removeIcon.IconPath = IconResourceConstants.RemoveDisabled;
          removeIcon.IsEnabled = false;
        }
        else
        {
          removeIcon.IconPath = VSThemeCommand.GetIgnoreIconEnabled();
          removeIcon.IsEnabled = true;
        }

        removeIcon = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RemoveIcon"));
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
