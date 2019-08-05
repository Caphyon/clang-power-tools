using ClangPowerTools.MVVM.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class TidySettingsViewModel : INotifyPropertyChanged
  {
    #region Members
    private TidySettingsModel tidySettings = new TidySettingsModel();
    private ICommand addDataCommand;

    public event PropertyChangedEventHandler PropertyChanged;
    #endregion

    #region Constructors
    public TidySettingsViewModel()
    {
      CPTSettings.TidySettings = tidySettings;
    }
    #endregion


    #region Properties
    public ICommand AddDataCommand
    {
      get => addDataCommand ?? (addDataCommand = new RelayCommand(() => OpenDataDialog(), () => CanExecute));
    }

    public bool CanExecute
    {
      get
      {
        return true;
      }
    }

    public void OpenDataDialog()
    {
      MessageBox.Show("Hello, world!");
      CPTSettings cPTSettings = new CPTSettings();
      cPTSettings.CheckOldSettings();
    }

    public string HeaderFilter
    {
      get
      {
        return tidySettings.HeaderFilter;
      }
      set
      {
        tidySettings.HeaderFilter = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HeaderFilter"));
      }
    }

    public IEnumerable<ClangTidyUseChecksFrom> UseChecksFrom
    {
      get
      {
        return Enum.GetValues(typeof(ClangTidyUseChecksFrom)).Cast<ClangTidyUseChecksFrom>();
      }
    }

    public ClangTidyUseChecksFrom SelectedUseChecksFrom
    {
      get
      {
        return tidySettings.UseChecksFrom;
      }
      set
      {
        tidySettings.UseChecksFrom = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedUseChecksFrom"));
      }
    }

    public string PredefinedChecks
    {
      get
      {
        return tidySettings.PredefinedChecks;
      }
      set
      {
        tidySettings.PredefinedChecks = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PredefinedChecks"));
      }
    }

    public string CustomChecks
    {
      get
      {
        return tidySettings.CustomChecks;
      }
      set
      {
        tidySettings.CustomChecks = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CustomChecks"));
      }
    }

    public string CustomExecutable
    {
      get
      {
        return tidySettings.CustomExecutable;
      }
      set
      {
        tidySettings.CustomExecutable = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CustomExecutable"));
      }
    }


    public bool FormatAfterTidy
    {
      get
      {
        return tidySettings.FormatAfterTidy;
      }
      set
      {
        tidySettings.FormatAfterTidy = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FormatAfterTidy"));
      }
    }

    public bool TidyOnSave
    {
      get
      {
        return tidySettings.TidyOnSave;
      }
      set
      {
        tidySettings.TidyOnSave = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TidyOnSave"));
      }
    }
    #endregion
  }
}
