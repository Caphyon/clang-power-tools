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

    private TidySettingsModel tidySettingsModel;
    private TidyChecksView tidyChecksView;
    private ICommand addDataCommand;
    private ICommand browseCommand;
    private ICommand selectCommand;
    private ICommand exportTidyConfigCommand;
    #endregion

    #region Constructor
    public TidySettingsViewModel()
    {
      tidySettingsModel = SettingsModelProvider.TidySettings;
    }
    #endregion

    #region Properties
    public ICommand AddDataCommand
    {
      get => addDataCommand ?? (addDataCommand = new RelayCommand(() => HeaderFilter = OpenContentDialog(HeaderFilter), () => CanExecute));
    }

    public ICommand BrowseCommand
    {
      get => browseCommand ?? (browseCommand = new RelayCommand(() => CustomExecutable = OpenFile(string.Empty, ".exe", "Executable files|*.exe"), () => CanExecute));
    }

    public ICommand SelectCommand
    {
      get => selectCommand ?? (selectCommand = new RelayCommand(() => OpenChecksWindow(), () => CanExecute));
    }

    public ICommand ExportTidyConfigCommand
    {
      get => exportTidyConfigCommand ?? (exportTidyConfigCommand = new RelayCommand(() => ExportTidyConfig(), () => CanExecute));
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
        return tidySettingsModel.HeaderFilter;
      }
      set
      {
        tidySettingsModel.HeaderFilter = value;
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
        return tidySettingsModel.UseChecksFrom;
      }
      set
      {
        tidySettingsModel.UseChecksFrom = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedUseChecksFrom"));
      }
    }

    public string Checks
    {
      get
      {
        return tidySettingsModel.Checks;
      }
      set
      {
        tidySettingsModel.Checks = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Checks"));
      }
    }

    public string CustomExecutable
    {
      get
      {
        return tidySettingsModel.CustomExecutable;
      }
      set
      {
        tidySettingsModel.CustomExecutable = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CustomExecutable"));
      }
    }

    public bool FormatAfterTidy
    {
      get
      {
        return tidySettingsModel.FormatAfterTidy;
      }
      set
      {
        tidySettingsModel.FormatAfterTidy = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FormatAfterTidy"));
      }
    }

    public bool TidyOnSave
    {
      get
      {
        return tidySettingsModel.TidyOnSave;
      }
      set
      {
        tidySettingsModel.TidyOnSave = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TidyOnSave"));
      }
    }
    #endregion

    #region Methods
    private void SetEnvironmentVariableTidyPath()
    {
      // TODO use method

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
      Checks = GetSelectedChecks();
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
