using System.Collections.Generic;
using System.ComponentModel;

namespace ClangPowerTools
{
  class TidyChecksViewModel : INotifyPropertyChanged
  {
    #region Members
    public event PropertyChangedEventHandler PropertyChanged;

    private string checkDescription = string.Empty;
    private TidyCheckModel selectedCheck = new TidyCheckModel();
    #endregion

    #region Properties
    public List<TidyCheckModel> TidyChecksList
    {
      get
      {
        return TidyChecks.checks;
      }
      set
      {
        TidyChecks.checks = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TidyChecksList"));
      }
    }

    public string CheckDescription
    {
      get
      {
        return checkDescription;
      }
      set
      {
        checkDescription = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CheckDescription"));
      }
    }

    public TidyCheckModel SelectedCheck
    {
      get
      {
        return selectedCheck;
      }
      set
      {
        selectedCheck = value;
        CheckDescription = "Description: " + selectedCheck.Description;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedCheck"));
      }
    }
    #endregion
  }
}
