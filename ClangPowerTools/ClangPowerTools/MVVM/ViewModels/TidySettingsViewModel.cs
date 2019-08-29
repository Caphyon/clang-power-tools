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

    private TidySettingsModel tidyModel = new TidySettingsModel();
    private TidyChecksView tidyChecksView;
    private string headerFilter = string.Empty;
    private ICommand headerFilterAddDataCommand;
    private ICommand customExecutableBrowseCommand;
    private ICommand predefinedChecksSelectCommand;
    private ICommand customChecksAddDataCommand;
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

    public List<string> HeaderFilters { get; set; }

    public string HeaderFilter
    {
      get
      {
        return headerFilter;
      }
      set
      {
        headerFilter = value;
        if (headerFilter == ComboBoxConstants.kCorrespondingHeaderName)
        {
          tidyModel.HeaderFilter = ComboBoxConstants.kCorrespondingHeaderValue;
        }
        else
        {
          tidyModel.HeaderFilter = value;
        }
      }
    }      

    public bool CanExecute
    {
      get
      {
        return true;
      }
    }

    public IEnumerable<ClangTidyUseChecksFrom> UseChecksFromItems
    {
      get
      {
        return Enum.GetValues(typeof(ClangTidyUseChecksFrom)).Cast<ClangTidyUseChecksFrom>();
      }
    }
    #endregion

    #region Constructor
    public TidySettingsViewModel()
    {
      HeaderFilters = new List<string>() { tidyModel.HeaderFilter, ComboBoxConstants.kCorrespondingHeaderName };
      headerFilter = tidyModel.HeaderFilter;    
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

    public ICommand PredefinedChecksSelectCommand
    {
      get => predefinedChecksSelectCommand ?? (predefinedChecksSelectCommand = new RelayCommand(() => UpdatePredefinedChecks(), () => CanExecute));
    }

    public ICommand CustomChecksAddDataCommand
    {
      get => customChecksAddDataCommand ?? (customChecksAddDataCommand = new RelayCommand(() => UpdateCustomChecks(), () => CanExecute));
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

    private void UpdateCustomChecks()
    {
      tidyModel.CustomChecks = OpenContentDialog(tidyModel.CustomChecks);
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TidyModel"));
    }

    private void UpdateCustomExecutable()
    {
      tidyModel.CustomExecutable = OpenFile(string.Empty, ".exe", "Executable files|*.exe");
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TidyModel"));
      SetEnvironmentVariableTidyPath();
    }

    private void UpdatePredefinedChecks()
    {
      OpenChecksWindow();
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TidyModel"));
    }

    private void SetEnvironmentVariableTidyPath()
    {
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
      TidyModel.PredefinedChecks = GetSelectedChecks();
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
      List<TidyCheckModel> selectedChecks = SettingsViewModelProvider.TidyChecksViewModel.SelectedChecks;

      foreach (TidyCheckModel item in selectedChecks)
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
