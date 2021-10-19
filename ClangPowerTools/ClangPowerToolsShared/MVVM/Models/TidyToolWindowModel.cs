using System.ComponentModel;

namespace ClangPowerToolsShared.MVVM.Models
{
  class TidyToolWindowModel
  {
    #region Members

    public event PropertyChangedEventHandler PropertyChanged;

    private bool canExecuteCommand = true;
    private bool selectedAll;
    private int tidyNr = 0;
    private int fixNr = 0;
    private int removeNr = 0;
    private int discardNr = 0;

    #endregion

    #region Properties

    public bool CanExecuteCommand
    {
      get
      {
        return canExecuteCommand;
      }

      set
      {
        canExecuteCommand = value;
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

    #endregion
  }
}
