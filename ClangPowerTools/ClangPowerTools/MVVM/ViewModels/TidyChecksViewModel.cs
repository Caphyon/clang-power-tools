using ClangPowerTools.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ClangPowerTools
{
  public class TidyChecksViewModel : INotifyPropertyChanged
  {
    #region Members

    public event PropertyChangedEventHandler PropertyChanged;

    private string checkSearch = string.Empty;
    private TidySettingsModel tidyModel;
    private TidyChecksView tidyChecksView;
    private SettingsProvider settingsProvider = new SettingsProvider();
    private TidyCheckModel selectedCheck = new TidyCheckModel();
    private List<TidyCheckModel> tidyChecksList = new List<TidyCheckModel>();

    #endregion

    #region Constructor

    public TidyChecksViewModel(TidyChecksView view)
    {
      var settingsProvider = new SettingsProvider();
      tidyModel = settingsProvider.GetTidySettingsModel();

      tidyChecksView = view;
      tidyChecksView.Closed += OnClosed;

      InitializeChecks();
    }

    #endregion


    #region Properties

    public TidyChecksView TidyChecksView
    {
      get
      {
        return tidyChecksView;
      }
      set
      {
        InitializeChecks();
        tidyChecksView = value;
      }
    }

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

    #endregion

    #region Methods

    public string GetSelectedChecks()
    {
      var checks = new StringBuilder();

      foreach (TidyCheckModel item in tidyChecksList)
      {
        if (item.IsChecked)
        {
          checks.Append(item.Name).Append(";");
        }
      }

      checks.Length--;
      return checks.ToString();
    }

    private void TickPredefinedChecks()
    {
      string input = settingsProvider.GetTidySettingsModel().PredefinedChecks;
      input = Regex.Replace(input, @"\s+", "");
      input = input.Remove(input.Length - 1, 1);
      var checkNames = input.Split(';').ToList();

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

    private void InitializeChecks()
    {
      string predefinedChecks = settingsProvider.GetTidySettingsModel().PredefinedChecks;

      if (string.IsNullOrEmpty(predefinedChecks))
      {
        var tidyChecks = new TidyChecks();
        tidyChecksList = new List<TidyCheckModel>(tidyChecks.Checks);
      }
      else
      {
        var tidyChecksClean = new TidyChecksClean();
        tidyChecksList = new List<TidyCheckModel>(tidyChecksClean.Checks);
        TickPredefinedChecks();
      }
    }

    private void OnClosed(object sender, EventArgs e)
    {
      tidyModel.PredefinedChecks = GetSelectedChecks();
      tidyChecksView.Closed -= OnClosed;
    }

    #endregion
  }
}
