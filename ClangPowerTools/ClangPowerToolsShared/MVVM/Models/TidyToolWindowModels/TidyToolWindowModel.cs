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
      //Init
      CountFilesModel = new CountFilesModel();

      //Register events
      CountFilesModel.PropertyChanged += UpdateDiscardFixIcon;

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

    public IconModel RemoveIcon { get; set; }

    /// <summary>
    /// Update discard fix icon with current values
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void UpdateDiscardFixIcon(object sender, System.EventArgs e)
    {
      DiscardFixIcon = discardFixIcon;
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
