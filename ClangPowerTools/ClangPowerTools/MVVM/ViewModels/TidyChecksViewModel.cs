using ClangPowerTools.MVVM.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class TidyChecksViewModel : INotifyPropertyChanged
  {
    #region Members
    public event PropertyChangedEventHandler PropertyChanged;

    private string checkSearch = string.Empty;
    private TidyCheckModel selectedCheck = new TidyCheckModel();
    private List<TidyCheckModel> tidyChecksList = new List<TidyCheckModel>(TidyChecks.Checks);
    private ICommand okCommand;
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
        return tidyChecksList.Where(e => e.Name.Contains(checkSearch, StringComparison.OrdinalIgnoreCase)).ToList();
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

    public TidyCheckModel SelectedCheck
    {
      get
      {
        return selectedCheck;
      }
      set
      {
        selectedCheck = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedCheck"));
      }
    }

    public bool CanExecute
    {
      get
      {
        return true;
      }
    }
    #endregion

    #region Commands
    public ICommand OkCommand
    {
      get => okCommand ?? (okCommand = new RelayCommand(() => UpdateSelectedCommands(), () => CanExecute));
    }
    #endregion

    #region Methods
    private void UpdateSelectedCommands()
    {

    }
    #endregion
  }
}
