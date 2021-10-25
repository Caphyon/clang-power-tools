using ClangPowerToolsShared.MVVM.Constants;
using System.ComponentModel;

namespace ClangPowerTools.MVVM.Models
{
  public class FileModel : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    #region Members

    private bool isRunning;
    private bool isEnabled;
    private bool isFixed;
    private bool isChecked;
    private string diffVisibility;
    private string fixVisibility;

    #endregion

    #region Constructor

    public FileModel()
    {
      DiffVisibility = UIElementsConstants.Hidden;
      FixVisibility = UIElementsConstants.Visibile;
      IsEnabled = true;
    }

    #endregion

    #region Public Members

    public string FileName { get; set; }
    public string FullFileName { get; set; }
    public string CopyFullFileName { get; set; }
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
        }
        else
        {
          DiffVisibility = UIElementsConstants.Hidden;
          FixVisibility = UIElementsConstants.Visibile;
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
        isRunning = value;
        IsEnabled = !value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsRunning"));
      }
    }

    public bool IsEnabled { 
      get { return isEnabled; }
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

    #endregion

  }
}
