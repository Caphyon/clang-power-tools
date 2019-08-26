using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace ClangPowerTools
{
  public class TidyChecksViewModel : INotifyPropertyChanged
  {
    #region Members
    public event PropertyChangedEventHandler PropertyChanged;

    private string checkDescription = string.Empty;
    private string checkSearch = string.Empty;
    private TidyCheckModel selectedCheck = new TidyCheckModel();
    private List<TidyCheckModel> tidyChecksList = new List<TidyCheckModel>(TidyChecks.Checks);
    #endregion

    #region Properties
    public List<TidyCheckModel> TidyChecksList
    {
      get
      {
        if (string.IsNullOrEmpty(checkSearch))
        {
          return tidyChecksList;
        }
        return tidyChecksList.Where(e => e.Name.Contains(checkSearch)).ToList();
      }
    }

    public string CheckSearch
    {
      get
      {
        return checkSearch;
      }
      set
      {
        checkSearch = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CheckSearch"));
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
        if (selectedCheck != null)
        {
          CheckDescription = "Description: " + selectedCheck.Description;
          PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedCheck"));
        }
      }
    }
    #endregion
  }
}
