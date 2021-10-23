using System.ComponentModel;

namespace ClangPowerToolsShared.MVVM.Models
{
  public class TidyToolWindowModel
  {
    #region Members

    public event PropertyChangedEventHandler PropertyChanged;

    private readonly string hidden = "Hidden";
    private readonly string visibile = "Visible";

    private bool isRunning;
    private bool selectedAll;
    private string progressBarVisibility;
    private string buttonVisibility;
    private int tidyNr = 0;
    private int fixedNr = 0;
    private int removeNr = 0;
    private int discardNr = 0;
    private int totalChecked = 0;
    private bool isChecked;
    public bool isDiscardEnabled;
    #endregion

    #region 

    public TidyToolWindowModel()
    {
      ProgressBarVisibility = hidden;
      ButtonVisibility = visibile;
    }

    #endregion

    #region Properties

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
      get {  return totalChecked; }
      set
      {
        totalChecked = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TotalChecked"));
      }
    }

    //public bool IsDiscardEnabled
    //{
    //  get
    //  {
    //    if(IsEnabled)
    //    {

    //    }
    //    return !IsEnabled ? false : true;
    //  }
    //  set
    //  {

    //    IsDiscardEnabled = value;
    //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsDiscardEnabled"));
    //  }
    //}

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
        if(totalChecked is not 0)
          return !isRunning;
        else
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
          ProgressBarVisibility = visibile;
          ButtonVisibility = hidden;
        }
        else
        {
          ProgressBarVisibility = hidden;
          ButtonVisibility = visibile;
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

    public string ButtonVisibility
    {
      get { return buttonVisibility; }
      set
      {
        buttonVisibility = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ButtonVisibility"));
      }
    }

    #endregion
  }
}
