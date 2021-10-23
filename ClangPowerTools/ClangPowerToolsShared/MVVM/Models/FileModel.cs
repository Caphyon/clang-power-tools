using System.ComponentModel;

namespace ClangPowerTools.MVVM.Models
{
  public class FileModel : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    #region Members

    private readonly string hidden = "Hidden";
    private readonly string visibile = "Visible";

    private bool isRunning;
    private bool isFixed;
    private bool isChecked;
    private string diffVisibility;
    private string fixVisibility;
    #endregion

    #region Constructor

    public FileModel()
    {
      DiffVisibility = hidden;
      FixVisibility = visibile;
    }

    #endregion

    #region Public Members

    public string FileName { get; set; }
    public string FullFileName { get; set; }
    public string CopyFullFileName { get; set; }
    public string DiffVisibility {
      get { return diffVisibility; } 
      set
      {
        diffVisibility = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DiffVisibility"));
      }
    }
    public string FixVisibility {
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
        return isFixed; } 
      set 
      {
        if(value)
        {
          DiffVisibility = visibile;
          FixVisibility = hidden;
        }  
        else
        {
          DiffVisibility = hidden;
          FixVisibility = visibile;
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
      }
    }

    public bool IsEnabled 
    { 
      get;
      set;
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
