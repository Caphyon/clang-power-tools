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
      //init private icons
      discardFixIcon = new IconModel(IconResourceConstants.DiscardFixDisabled, UIElementsConstants.Visibile, false);
      tidyFixIcon = new IconModel(VSThemeCommand.GetTidyFixIconEnabled(), UIElementsConstants.Visibile, true);
      refreshTidyIcon = new IconModel(VSThemeCommand.GetRefreshTidyIconEnabled(), UIElementsConstants.Visibile, true);
      removeIcon = new IconModel(VSThemeCommand.GetIgnoreIconEnabled(), UIElementsConstants.Visibile, true);

      //Init
      CountFilesModel = new CountFilesModel();

      //Register events
      CountFilesModel.PropertyChanged += UpdateIconsOnPropertyChange;

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
        discardFixIcon.Tooltip = $"Discard {CountFilesModel.TotalCheckedFixedFiles} files";
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
        tidyFixIcon.Tooltip = $"Fix {CountFilesModel.TotalCheckedSourceFiles - CountFilesModel.TotalCheckedFixedSouceFiles} source files";
        tidyFixIcon = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TidyFixIcon"));
      }
    }

    private IconModel refreshTidyIcon;
    public IconModel RefreshTidyIcon
    {
      get { return refreshTidyIcon; }
      set
      {
        if (CountFilesModel.TotalCheckedFiles == 0 ||
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

        refreshTidyIcon.Tooltip = $"Refresh tidy {CountFilesModel.TotalCheckedSourceFiles} source files";
        refreshTidyIcon = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RefreshTidyIcon"));
      }
    }

    private IconModel removeIcon { get; set; }
    public IconModel RemoveIcon
    {
      get { return removeIcon; }
      set
      {
        if (CountFilesModel.TotalCheckedFiles == 0)
        {
          removeIcon.IconPath = IconResourceConstants.RemoveDisabled;
          removeIcon.IsEnabled = false;
        }
        else
        {
          removeIcon.IconPath = VSThemeCommand.GetIgnoreIconEnabled();
          removeIcon.IsEnabled = true;
        }

        removeIcon.Tooltip = $"Ignore {CountFilesModel.TotalCheckedFiles} files";
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

    /// <summary>
    /// Change theme just for enabled icons
    /// </summary>
    public void ChangeIconsTheme()
    {
      RefreshTidyIcon.IconPath = RefreshTidyIcon.IsEnabled == true ?
        VSThemeCommand.GetRefreshTidyIconEnabled() : IconResourceConstants.DiffDisabled;
      TidyFixIcon.IconPath = TidyFixIcon.IsEnabled == true ?
        VSThemeCommand.GetTidyFixIconEnabled() : IconResourceConstants.FixDisabled;
      DiscardFixIcon.IconPath = DiscardFixIcon.IsEnabled == true ?
        VSThemeCommand.GetDiscardFixIconEnabled() : IconResourceConstants.FixDisabled;
      RemoveIcon.IconPath = RemoveIcon.IsEnabled == true ?
        VSThemeCommand.GetIgnoreIconEnabled() : IconResourceConstants.FixDisabled;
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

    #endregion
  }
}
