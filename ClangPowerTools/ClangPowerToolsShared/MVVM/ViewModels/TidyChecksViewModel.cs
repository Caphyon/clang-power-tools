using ClangPowerTools.Events;
using ClangPowerTools.MVVM;
using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
    private TidyCheckModel selectedCheck = new();
    private List<TidyCheckModel> tidyChecksList = new();
    private ICommand resetSearchCommand;

    #endregion

    #region Constructor

    public TidyChecksViewModel(TidyChecksView view)
    {
      tidyModel = SettingsProvider.TidySettingsModel;

      tidyChecksView = view;
      tidyChecksView.Closed += OnClosed;

      // Click event is used because the Check value is changed many time from the code
      // In this way we don't need to make more checks to see from where the Check event was triggered 
      tidyChecksView.EnableDisableAll.Click += (object sender, RoutedEventArgs e) =>
      {
        // Check event is triggered before Click event. 
        // IsChecked property will already have the new value when the Click event will happend 
        EnableDisableAllChecks(tidyChecksView.EnableDisableAll.IsChecked == true ? true : false);
      };

      tidyChecksView.EnableDisableDefaults.Click += (object sender, RoutedEventArgs e) =>
      {
        // Check event is triggered before Click event. 
        // IsChecked property will already have the new value when the Click event will happend 
        SetDefaultsToggle(tidyChecksView.EnableDisableDefaults.IsChecked == true ? true : false);
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

        // Always keep the current list of checks under surveillance
        CollectionElementsCounter.Initialize(checks);
        CollectionElementsCounter.StateEvent += SetStateForEnableDisableAllButton;

        SetInitialStateEnableAllToggle(checks);

        return checks;
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

    public ICommand ResetSearchCommand
    {
      get => resetSearchCommand ??= new RelayCommand(() => ResetSearchField(), () => CanExecute);
    }

    #endregion


    #region Methods

    public void MultipleStateChange(bool checkValue)
    {
      if (tidyChecksView.TidyChecksListBox.SelectedItems.Count <= 1)
        return;

      foreach (object item in tidyChecksView.TidyChecksListBox.SelectedItems)
      {
        TidyCheckModel model = (TidyCheckModel)item;
        if (model.IsChecked != checkValue)
        {
          model.IsChecked = checkValue;
        }
      }
    }

    public void DeactivateDefaultsToggle()
    {
      if (tidyChecksView.EnableDisableDefaults.IsChecked.Value)
      {
        tidyChecksView.EnableDisableDefaults.IsChecked = false;
      }
    }

    public void OpenBrowser(string tidyCheckName)
    {
      string uri = CreateFlagUri(tidyCheckName);
      Process.Start(uri);
    }

    private string CreateFlagUri(string tidyCheckName)
    {
      StringBuilder sb = new();
      string checkName = string.Empty;
      if (tidyCheckName.Contains("clang-analyzer-"))
      {
        checkName = tidyCheckName.Replace("clang-analyzer-", "clang-analyzer/");
      }
      else if (tidyCheckName.IndexOf('-') != -1)
      {
        var listTidyCheckName = tidyCheckName.ToCharArray();
        listTidyCheckName[tidyCheckName.IndexOf('-')] = '/';
        checkName = new string(listTidyCheckName);
      }
      sb.Append(TidyConstants.FlagsUri).Append(checkName).Append(".html");
      return sb.ToString();
    }

    private void LoadChecks()
    {
      string input = SettingsProvider.TidySettingsModel.PredefinedChecks;

      if (string.IsNullOrWhiteSpace(input))
      {
        return;
      }

      input = Regex.Replace(input, @"\s+", "");
      input = input.Remove(input.Length - 1, 1);
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

    private void InitializeChecks()
    {
      tidyChecksList = new TidyChecks().Checks;
      LoadChecks();
      SetInitialStateEnableAllToggle(tidyChecksList);
      SetInitialStateDefaultsToggle();
    }

    private void SetDefaultsToggle(bool value)
    {
      if (value)
      {
        SetDefaultChecks();
        tidyChecksView.EnableDisableDefaults.IsChecked = true;
      }
      else
      {
        EnableDisableAllChecks(false);
        tidyChecksView.EnableDisableDefaults.IsChecked = false;
      }
    }

    private void SetInitialStateDefaultsToggle()
    {
      int count = tidyChecksList.Where(e => e.IsChecked).Count();
      if (count == 0 || count != TidyChecksDefault.Checks.Count)
      {
        tidyChecksView.EnableDisableDefaults.IsChecked = false;
        return;
      }

      foreach (TidyCheckModel check in tidyChecksList)
      {
        if (check.IsChecked == false && TidyChecksDefault.Checks.Contains(check.Name))
        {
          tidyChecksView.EnableDisableDefaults.IsChecked = false;
          return;
        }
      }

      tidyChecksView.EnableDisableDefaults.IsChecked = true;
    }


    /// <summary>
    /// Enable or disable all the tidy checks from the current tidy checks list 
    /// </summary>
    /// <param name="value">True to enable all tidy checks. False to disable all tidy checks</param>
    private void EnableDisableAllChecks(bool value)
    {
      // get all checks from current view considering the search filter
      var checks = string.IsNullOrEmpty(checkSearch) ? tidyChecksList :
          tidyChecksList.Where(e => e.Name.Contains(checkSearch, StringComparison.OrdinalIgnoreCase)).ToList();

      // set just the current collection of checks
      for (int i = 0; i < checks.Count; ++i)
        checks[i].IsChecked = value;
    }

    /// <summary>
    /// Set the state for Enable/Disable All toggle button
    /// </summary>
    /// <param name="checks">Tidy checks collection</param>
    private void SetInitialStateEnableAllToggle(IEnumerable<TidyCheckModel> checks)
    {
      // to avoid enter in the second condition the first one must be split in two if statements
      // uncheck the Enable All toggle button if the retured list of checks has 0 elements
      if (checks.Count() == 0)
      {
        if (tidyChecksView.EnableDisableAll.IsChecked == true)
          tidyChecksView.EnableDisableAll.IsChecked = false;
      }

      // check the Enable All toggle button if all the checks from the current view are enabled
      else if (tidyChecksView.EnableDisableAll.IsChecked == false && !checks.Any(c => c.IsChecked == false))
      {
        tidyChecksView.EnableDisableAll.IsChecked = true;
      }

      // uncheck the Enable All toggle button if any check from the list is disabled
      else if (tidyChecksView.EnableDisableAll.IsChecked == true && checks.Any(c => c.IsChecked == false))
      {
        tidyChecksView.EnableDisableAll.IsChecked = false;
      }
    }

    /// <summary>
    /// Set the state for Enable/Disable All toggle button
    /// </summary>
    /// <param name="sender">Value is NULL. Event is triggered from a static object which has no this value.</param>
    /// <param name="e">Contains the state of the toggle button</param>
    private void SetStateForEnableDisableAllButton(object sender, BoolEventArgs e)
    {
      tidyChecksView.EnableDisableAll.IsChecked = e.Value;
    }

    private void OnClosed(object sender, EventArgs e)
    {
      tidyModel.PredefinedChecks = GetSelectedChecks();
      tidyChecksView.Closed -= OnClosed;

      CollectionElementsCounter.StateEvent -= SetStateForEnableDisableAllButton;
    }

    private string GetSelectedChecks()
    {
      StringBuilder checks = new();

      foreach (TidyCheckModel item in tidyChecksList)
      {
        if (item.IsChecked)
        {
          checks.Append(item.Name).Append(";");
        }
      }

      return checks.ToString();
    }

    private void ResetSearchField()
    {
      CheckSearch = string.Empty;
    }

    private void SetDefaultChecks()
    {
      foreach (TidyCheckModel check in tidyChecksList)
      {
        check.IsChecked = TidyChecksDefault.Checks.Contains(check.Name);
      }
    }

    #endregion
  }
}
