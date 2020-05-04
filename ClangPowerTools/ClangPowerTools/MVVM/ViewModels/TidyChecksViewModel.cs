﻿using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class TidyChecksViewModel : INotifyPropertyChanged
  {
    #region Members

    public event PropertyChangedEventHandler PropertyChanged;

    private string checkSearch = string.Empty;
    private TidySettingsModel tidyModel;
    private TidyChecksView tidyChecksView;
    private TidyCheckModel selectedCheck = new TidyCheckModel();
    private List<TidyCheckModel> tidyChecksList = new List<TidyCheckModel>();
    private ICommand resetSearchCommand;
    //private bool selectAllFlag = false;

    #endregion

    #region Constructor

    public TidyChecksViewModel(TidyChecksView view)
    {
      tidyModel = SettingsProvider.TidySettingsModel;

      tidyChecksView = view;
      tidyChecksView.Closed += OnClosed;

      tidyChecksView.SelectAllCheckBox.Checked +=
        (object sender, RoutedEventArgs e) =>
          {
            //selectAllFlag = true;
            SelectOrDeselectAll(true);
          };

      tidyChecksView.SelectAllCheckBox.Unchecked +=
        (object sender, RoutedEventArgs e) =>
          {
            //selectAllFlag = false;
            SelectOrDeselectAll(false);
          };

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
        List<TidyCheckModel> checks = string.IsNullOrEmpty(checkSearch) ? tidyChecksList :
          tidyChecksList.Where(e => e.Name.Contains(checkSearch, StringComparison.OrdinalIgnoreCase)).ToList();

        CheckSelectAllButton(checks);

        return checks;
      }
    }

    private void SelectOrDeselectAll(bool value)
    {
      var checks = string.IsNullOrEmpty(checkSearch) ? tidyChecksList :
          tidyChecksList.Where(e => e.Name.Contains(checkSearch, StringComparison.OrdinalIgnoreCase)).ToList();

      for (int i = 0; i < checks.Count; ++i)
      {
        checks[i].IsChecked = value;
      }

      //checks.ForEach(c => c.IsChecked = value);

    }

    private void CheckSelectAllButton(IEnumerable<TidyCheckModel> checks)
    {
      if (tidyChecksView.SelectAllCheckBox.IsChecked == false && !checks.Any(c => c.IsChecked == false))
      {
        tidyChecksView.SelectAllCheckBox.IsChecked = true;
        //selectAllFlag = true;
      }
      else
        tidyChecksView.SelectAllCheckBox.IsChecked = false;

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
    public ICommand ResetSearchCommand
    {
      get => resetSearchCommand ?? (resetSearchCommand = new RelayCommand(() => ResetSearchField(), () => CanExecute));
    }

    #endregion


    #region Methods



    private string GetSelectedChecks()
    {
      var checks = new StringBuilder();

      foreach (TidyCheckModel item in tidyChecksList)
      {
        if (item.IsChecked)
        {
          checks.Append(item.Name).Append(";");
        }
      }

      if (checks.Length != 0)
        checks.Length--;

      return checks.ToString();
    }

    private void TickPredefinedChecks()
    {
      string input = SettingsProvider.TidySettingsModel.PredefinedChecks;
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
      string predefinedChecks = SettingsProvider.TidySettingsModel.PredefinedChecks;

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

      CheckSelectAllButton(tidyChecksList);
    }

    private void OnClosed(object sender, EventArgs e)
    {
      tidyModel.PredefinedChecks = GetSelectedChecks();
      tidyChecksView.Closed -= OnClosed;
    }

    private void ResetSearchField()
    {
      CheckSearch = string.Empty;
    }

    #endregion
  }
}
