using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class TidyChecksViewModel : INotifyPropertyChanged
  {
    #region Members
    public event PropertyChangedEventHandler PropertyChanged;

    private string checkSearch = string.Empty;
    private TidyCheckModel selectedCheck = new TidyCheckModel();
    private List<TidyCheckModel> tidyChecksList = new List<TidyCheckModel>();
    private ICommand okCommand;
    #endregion

    #region Properties
    public TidyChecksView TidyChecksView { get; set; }

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

    public TidyChecksViewModel()
    {
      InitializeChecks();
      SettingsProvider.TidySettingsViewModel.TidyModel.PredefinedChecks = GetSelectedChecks();
    }

    private void InitializeChecks()
    {
      string predefinedChecks = SettingsProvider.TidySettingsViewModel.TidyModel.PredefinedChecks;

      if (string.IsNullOrEmpty(predefinedChecks))
      {
        tidyChecksList = new List<TidyCheckModel>(TidyChecks.Checks);
      }
      else
      {
        tidyChecksList = new List<TidyCheckModel>(TidyChecksClean.Checks);
        TickPredefinedChecks();
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
      get => okCommand ?? (okCommand = new RelayCommand(() => UpdateSelectedChecksCommand(), () => CanExecute));
    }
    #endregion


    #region Methods
    public string GetSelectedChecks()
    {
      StringBuilder stringBuilder = new StringBuilder();

      foreach (TidyCheckModel item in TidyChecksList)
      {
        if (item.IsChecked)
        {
          stringBuilder.Append(item.Name).Append(";");
        }
      }
      return stringBuilder.ToString();
    }

    private void UpdateSelectedChecksCommand()
    {
      TidyChecksView.Close();
    }

    private void TickPredefinedChecks()
    {
      string input = SettingsProvider.TidySettingsViewModel.TidyModel.PredefinedChecks;
      input = Regex.Replace(input, @"\s+", "");
      List<string> checkNames = input.Split(';').ToList();

      foreach (string check in checkNames)
      {
        foreach (TidyCheckModel tidyModel in tidyChecksList)
        {
          if (string.Equals(check, tidyModel.Name, StringComparison.OrdinalIgnoreCase))
          {
            tidyModel.IsChecked = true;
          }
        }
      }
    }

    #endregion
  }
}
