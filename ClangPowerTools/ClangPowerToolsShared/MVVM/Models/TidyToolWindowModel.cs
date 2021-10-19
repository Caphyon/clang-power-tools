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

    public bool CanExecuteCommand
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
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CanExecuteCommand"));
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
