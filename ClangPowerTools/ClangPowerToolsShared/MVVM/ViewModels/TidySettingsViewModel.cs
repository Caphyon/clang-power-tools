using ClangPowerTools.MVVM;
using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.Views;
using ClangPowerToolsShared.MVVM.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class TidySettingsViewModel : CommonSettingsFunctionality, INotifyPropertyChanged
  {
    #region Members
    public event PropertyChangedEventHandler PropertyChanged;

    private TidySettingsModel tidyModel;
    private string headerFilter = string.Empty;
    private string displayWarning = string.Empty;
    private ICommand headerFilterAddDataCommand;
    private ICommand customExecutableBrowseCommand;
    private ICommand compilationDatabaseDirBrowseCommand;
    private ICommand predefinedChecksSelectCommand;
    private ICommand customChecksAddDataCommand;
    private ICommand exportTidyConfigCommand;

    #endregion

    #region Constructor

    public TidySettingsViewModel()
    {
      tidyModel = SettingsProvider.TidySettingsModel;
      HeaderFilters = new List<string>() { tidyModel.HeaderFilter, ComboBoxConstants.kCorrespondingHeaderName };
      headerFilter = tidyModel.HeaderFilter;
      UpdateWarningVisibility();
    }

    #endregion

    #region Properties
    public TidySettingsModel TidyModel
    {
      get
      {
        UpdateWarningVisibility();
        return tidyModel;
      }
      set
      {
        tidyModel = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TidyModel"));
      }
    }

    public string DisplayWarning
    {
      get { return displayWarning; }
      set
      {
        displayWarning = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DisplayWarning"));
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

    public SettingsTooltips Tooltip { get; } = new SettingsTooltips();

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

    public ICommand CompilationDatabaseDirBrowseCommand
        {
      get => compilationDatabaseDirBrowseCommand ?? (compilationDatabaseDirBrowseCommand = new RelayCommand(() => UpdateCompilationDatabaseDir(), () => CanExecute));
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

    private void UpdateWarningVisibility()
    {
      var tidySettings = SettingsProvider.TidySettingsModel;
      if (tidySettings.ApplyTidyFix)
        displayWarning = UIElementsConstants.Visibile;
      else
        displayWarning = UIElementsConstants.Hidden;
      DisplayWarning = displayWarning;
    }

    private void UpdateHeaderFilter()
    {
      tidyModel.HeaderFilter = OpenContentDialog(tidyModel.HeaderFilter);
      HeaderFilter = tidyModel.HeaderFilter;
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TidyModel"));
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HeaderFilter"));
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
    }

    private void UpdateCompilationDatabaseDir()
    {
      tidyModel.CompilationDatabaseDir = BrowseForFolderFiles();
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TidyModel"));
    }

    private void UpdatePredefinedChecks()
    {
      OpenChecksWindow();
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TidyModel"));
    }

    private void ExportTidyConfig()
    {
      var tidyConfigFile = new TidyConfigFile();
      string fileName = ".clang-tidy";
      string defaultExt = ".clang-tidy";
      string filter = "Configuration files (.clang-tidy)|*.clang-tidy";

      string path = SaveFile(fileName, defaultExt, filter);
      if (string.IsNullOrEmpty(path) == false)
      {
        WriteContentToFile(path, tidyConfigFile.CreateOutput().ToString());
        MessageBox.Show(".clang-tidy file exported at the selected location.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
    }

    private void OpenChecksWindow()
    {
      var tidyChecksView = new TidyChecksView();
      tidyChecksView.ShowDialog();
    }

    #endregion
  }
}
