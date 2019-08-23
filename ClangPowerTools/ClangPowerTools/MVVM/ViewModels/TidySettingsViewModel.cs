using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class TidySettingsViewModel : CommonSettingsFunctionality, INotifyPropertyChanged
  {
    #region Members
    public event PropertyChangedEventHandler PropertyChanged;

    private TidySettingsModel tidyModel;
    private TidyChecksView tidyChecksView;
    private ICommand headerFilterAddDataCommand;
    private ICommand customExecutableBrowseCommand;
    private ICommand checksSelectCommand;
    private ICommand exportTidyConfigCommand;
    #endregion

    #region Properties
    public TidySettingsModel TidyModel
    {
      get
      {
        return tidyModel;
      }
      set
      {
        tidyModel = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TidyModel"));
      }
    }

    public bool CanExecute
    {
      get
      {
        return true;
      }
    }


    public IEnumerable<ClangTidyChecksFrom> UseChecksFromItems
    {
      get
      {
        return Enum.GetValues(typeof(ClangTidyChecksFrom)).Cast<ClangTidyChecksFrom>();
      }
    }

    #endregion

    #region Constructor
    public TidySettingsViewModel()
    {
      tidyModel = new TidySettingsModel();
      SettingsViewModelProvider.TidySettingsViewModel = this;
    }
    #endregion

    #region Commands
    public ICommand HeaderFilterAddDataCommand
    {
      get => headerFilterAddDataCommand ?? (headerFilterAddDataCommand = new RelayCommand(() => UpdateHeaderFilter(), () => CanExecute));
    }

    public ICommand CustomExecutableBrowseCommand
    {
      get => customExecutableBrowseCommand ?? (customExecutableBrowseCommand = new RelayCommand(() => UpdateCustomExecutable(), () => CanExecute));
    }

    public ICommand ChecksSelectCommand
    {
      get => checksSelectCommand ?? (checksSelectCommand = new RelayCommand(() => UpdateChecks(), () => CanExecute));
    }

    public ICommand ExportTidyConfigCommand
    {
      get => exportTidyConfigCommand ?? (exportTidyConfigCommand = new RelayCommand(() => ExportTidyConfig(), () => CanExecute));
    }

    #endregion

    #region Methods
    private void UpdateHeaderFilter()
    {
      tidyModel.HeaderFilter = OpenContentDialog(tidyModel.HeaderFilter);
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TidyModel"));
    }

    private void UpdateCustomExecutable()
    {
      tidyModel.CustomExecutable = OpenFile(string.Empty, ".exe", "Executable files|*.exe");
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TidyModel"));
      //SetEnvironmentVariableTidyPath();
    }

    private void UpdateChecks()
    {
      OpenChecksWindow();
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TidyModel"));
    }

    private void SetEnvironmentVariableTidyPath()
    {
      // TODO use method

      var task = Task.Run(() =>
      {
        if (TidyModel.CustomExecutable.Length > 3)
        {
          Environment.SetEnvironmentVariable(ScriptConstants.kEnvrionmentTidyPath, tidyModel.CustomExecutable, EnvironmentVariableTarget.User);
        }
        else
        {
          Environment.SetEnvironmentVariable(ScriptConstants.kEnvrionmentTidyPath, null, EnvironmentVariableTarget.User);
        }
      });
    }

    private void ExportTidyConfig()
    {
      TidyConfigFile tidyConfigFile = new TidyConfigFile();
      string fileName = ".clang-tidy";
      string defaultExt = ".clang-tidy";
      string filter = "Configuration files (.clang-tidy)|*.clang-tidy";

      string path = SaveFile(fileName, defaultExt, filter);
      if (string.IsNullOrEmpty(path) == false)
      {
        WriteContentToFile(path, tidyConfigFile.CreateOutput().ToString());
      }
    }

    private void OnClosed(object sender, EventArgs e)
    {
      TidyModel.Checks = GetSelectedChecks();
      tidyChecksView.Closed -= OnClosed;
    }

    private void OpenChecksWindow()
    {
      tidyChecksView = new TidyChecksView();
      tidyChecksView.Closed += OnClosed;
      tidyChecksView.ShowDialog();

    }

    private string GetSelectedChecks()
    {
      StringBuilder stringBuilder = new StringBuilder();

      foreach (TidyCheckModel item in TidyChecks.Checks)
      {
        if (item.IsChecked)
        {
          stringBuilder.Append(item.Name).Append(";");
        }
      }
      return stringBuilder.ToString();
    }

    #endregion
  }
}
