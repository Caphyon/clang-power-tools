using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class TidySettingsViewModel : CommonSettingsFunctionality, INotifyPropertyChanged
  {
    #region Members
    public event PropertyChangedEventHandler PropertyChanged;

    private ICommand addDataCommand;
    private ICommand browseCommand;
    private ICommand selectCommand;
    #endregion


    #region Properties
    public ICommand AddDataCommand
    {
      get => addDataCommand ?? (addDataCommand = new RelayCommand(() => HeaderFilter = OpenContentDialog(HeaderFilter), () => CanExecute));
    }

    public ICommand BrowseCommand
    {
      get => browseCommand ?? (browseCommand = new RelayCommand(() => CustomExecutable = BrowseForFile(), () => CanExecute));
    }

    public ICommand SelectCommand
    {
      get => selectCommand ?? (selectCommand = new RelayCommand(() => OpenChecksWindow(), () => CanExecute));
    }

    public bool CanExecute
    {
      get
      {
        return true;
      }
    }

    public string HeaderFilter
    {
      get
      {
        return SettingsModelHandler.TidySettings.HeaderFilter;
      }
      set
      {
        SettingsModelHandler.TidySettings.HeaderFilter = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HeaderFilter"));
      }
    }

    public IEnumerable<ClangTidyChecksFrom> UseChecksFrom
    {
      get
      {
        return Enum.GetValues(typeof(ClangTidyChecksFrom)).Cast<ClangTidyChecksFrom>();
      }
    }

    public ClangTidyChecksFrom SelectedUseChecksFrom
    {
      get
      {
        return SettingsModelHandler.TidySettings.UseChecksFrom;
      }
      set
      {
        SettingsModelHandler.TidySettings.UseChecksFrom = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedUseChecksFrom"));
      }
    }

    public string Checks
    {
      get
      {
        return SettingsModelHandler.TidySettings.Checks;
      }
      set
      {
        SettingsModelHandler.TidySettings.Checks = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Checks"));
      }
    }

    public string CustomExecutable
    {
      get
      {
        return SettingsModelHandler.TidySettings.CustomExecutable;
      }
      set
      {
        SettingsModelHandler.TidySettings.CustomExecutable = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CustomExecutable"));
      }
    }

    public bool FormatAfterTidy
    {
      get
      {
        return SettingsModelHandler.TidySettings.FormatAfterTidy;
      }
      set
      {
        SettingsModelHandler.TidySettings.FormatAfterTidy = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FormatAfterTidy"));
      }
    }

    public bool TidyOnSave
    {
      get
      {
        return SettingsModelHandler.TidySettings.TidyOnSave;
      }
      set
      {
        SettingsModelHandler.TidySettings.TidyOnSave = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TidyOnSave"));
      }
    }
    #endregion

    #region Methods
    private void SetEnvironmentVariableTidyPath()
    {
      var task = Task.Run(() =>
      {
        if (CustomExecutable.Length > 3)
        {
          Environment.SetEnvironmentVariable(ScriptConstants.kEnvrionmentTidyPath, CustomExecutable, EnvironmentVariableTarget.User);
        }
        else
        {
          Environment.SetEnvironmentVariable(ScriptConstants.kEnvrionmentTidyPath, null, EnvironmentVariableTarget.User);
        }
      });
    }

    private void OpenChecksWindow()
    {
      TidyChecksView tidyChecksView = new TidyChecksView();
      tidyChecksView.ShowDialog();
    }

    #endregion
  }
}
