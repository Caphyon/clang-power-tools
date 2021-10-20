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
    private int fixNr = 0;
    private int removeNr = 0;
    private int discardNr = 0;

    #endregion

    #region 

    public TidyToolWindowModel()
    {
      ProgressBarVisibility = hidden;
      ButtonVisibility = visibile;
    }

    #endregion

    #region Properties

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

    public int FixNr
    {
      get { return fixNr; }
      set
      {
        fixNr = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FixNr"));
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
