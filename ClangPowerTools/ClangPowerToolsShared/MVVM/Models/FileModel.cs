using ClangPowerToolsShared.MVVM.Commands;
using ClangPowerToolsShared.MVVM.Constants;
using ClangPowerToolsShared.MVVM.Models;
using ClangPowerToolsShared.MVVM.Models.TidyToolWindowModels;
using System.ComponentModel;

namespace ClangPowerTools.MVVM.Models
{
  public class FileModel : INotifyPropertyChanged
  {

    #region Members

    public event PropertyChangedEventHandler PropertyChanged;
    private bool isRunning;
    private bool isFixed;
    private bool isChecked;
    private string filename;

    #endregion

    #region Constructor

    public FileModel()
    {
      //init private icons
      diffIcon = new IconModel(VSThemeCommand.GetDiffIconEnabled(), UIElementsConstants.Hidden, false);
      tidyFixIcon = new IconModel(VSThemeCommand.GetTidyFixIconEnabled(), UIElementsConstants.Visibile, true);

      //init public icons
      DiffIcon = new IconModel(VSThemeCommand.GetDiffIconEnabled(), UIElementsConstants.Hidden, false);
      TidyFixIcon = new IconModel(VSThemeCommand.GetTidyFixIconEnabled(), UIElementsConstants.Visibile, true);
    }

    public FileModel(FileModel file)
    {
      //init private icons
      diffIcon = new IconModel(VSThemeCommand.GetDiffIconEnabled(), UIElementsConstants.Hidden, false);
      tidyFixIcon = new IconModel(VSThemeCommand.GetTidyFixIconEnabled(), UIElementsConstants.Visibile, true);

      //init public icons
      DiffIcon = new IconModel(VSThemeCommand.GetDiffIconEnabled(), UIElementsConstants.Hidden, false);
      TidyFixIcon = new IconModel(VSThemeCommand.GetTidyFixIconEnabled(), UIElementsConstants.Visibile, true);

      this.FileName = file.FileName;
      this.FullFileName = file.FullFileName;
      this.CopyFullFileName = file.CopyFullFileName;
      this.DiffIcon = file.DiffIcon;
      this.TidyFixIcon = file.TidyFixIcon;
      this.IsFixed = file.IsFixed;
      this.IsRunning = file.IsRunning;
      this.IsChecked = file.IsChecked;
      this.FilesType = file.FilesType;
    }

    #endregion

    #region Public Properities

    public FileType FilesType { get; set; }
    public string FilesTypeString
    {
      get
      {
        if (FilesType == FileType.SourceFile)
          return "Source files";
        else
          return "Affected headers";
      }
    }

    public string FileName
    {
      get { return filename; }
      set
      {
        filename = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FileName"));
      }

    }

    public string FullFileName { get; set; }
    public string CopyFullFileName { get; set; }

    private IconModel diffIcon;
    public IconModel DiffIcon
    {
      get; set;
    }

    private IconModel tidyFixIcon;
    public IconModel TidyFixIcon
    {
      get { return tidyFixIcon; }
      set
      {
        tidyFixIcon = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TidyFixIcon"));
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
          DiffIcon.Visibility = UIElementsConstants.Visibile;
          DiffIcon.IsEnabled = true;
          TidyFixIcon.Visibility = UIElementsConstants.Hidden;
          TidyFixIcon.IsEnabled = false;
        }
        else
        {
          TidyFixIcon.Visibility = UIElementsConstants.Visibile;
          TidyFixIcon.IsEnabled = true;
          DiffIcon.Visibility = UIElementsConstants.Hidden;
          DiffIcon.IsEnabled = false;
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
          TidyFixIcon.IsEnabled = false;
          TidyFixIcon.IconPath = IconResourceConstants.FixDisabled;
        }
        else
        {
          TidyFixIcon.IsEnabled = true;
          TidyFixIcon.IconPath = VSThemeCommand.GetTidyFixIconEnabled();
        }
        isRunning = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsRunning"));
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

    //private void SelectIconsDarkTheme()
    //{
    //  TidyFixIcon.IconPath = IconResourceConstants.FixDark;
    //  DiffIcon.IconPath = IconResourceConstants.DiffDark;
    //}

    /// <summary>
    /// Disable and show (maake visible) diff icon
    /// </summary>
    //public void DisableVisibleDiffIcon()
    //{
    //  DiffIcon.Visibility = UIElementsConstants.Visibile;
    //  TidyFixIcon.Visibility = UIElementsConstants.Hidden;
    //  DiffIcon.IconPath = IconResourceConstants.DiffDisabled;
    //  DiffIcon.IsEnabled = false;
    //}

    //public void EnableDiffIcon()
    //{
    //  SelectEnableIcons();
    //  DiffIcon.Visibility = UIElementsConstants.Visibile;
    //  DiffIcon.IsEnabled = true;
    //  TidyFixIcon.Visibility = UIElementsConstants.Hidden;
    //}

    //public void EnableFixIcon()
    //{
    //  SelectEnableIcons();
    //  TidyFixIcon.Visibility = UIElementsConstants.Visibile;
    //  TidyFixIcon.IsEnabled = true;
    //  DiffIcon.Visibility = UIElementsConstants.Hidden;
    //}

    //public void DisableVisibleTidyFixIcon()
    //{
    //  TidyFixIcon.Visibility = UIElementsConstants.Visibile;
    //  DiffIcon.Visibility = UIElementsConstants.Hidden;
    //  TidyFixIcon.IconPath = IconResourceConstants.FixDisabled;
    //  TidyFixIcon.IsEnabled = false;
    //}

    //private void SelectIconsLightTheme()
    //{
    //  TidyFixIcon.IconPath = IconResourceConstants.FixLight;
    //  DiffIcon.IconPath = IconResourceConstants.DiffLight;
    //}

    //private void SelectDisableIcons()
    //{
    //  TidyFixIcon.IconPath = IconResourceConstants.FixDisabled;
    //  DiffIcon.IconPath = IconResourceConstants.DiffDisabled;
    //  TidyFixIcon.IsEnabled = false;
    //  DiffIcon.IsEnabled = false;
    //}

    //public void SelectEnableIcons()
    //{
    //  if (VSThemeCommand.GetCurrentVsTheme() == VsThemes.Dark)
    //    SelectIconsDarkTheme();
    //  else
    //    SelectIconsLightTheme();
    //}

    #endregion

  }
}
