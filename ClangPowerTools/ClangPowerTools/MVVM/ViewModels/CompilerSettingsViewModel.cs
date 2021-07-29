using ClangPowerTools.MVVM;
using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class CompilerSettingsViewModel : CommonSettingsFunctionality, INotifyPropertyChanged
  {
    #region Members

    public event PropertyChangedEventHandler PropertyChanged;

    private CompilerSettingsModel compilerModel;
    private readonly PowerShellService powerShellService;

    private ICommand compileFlagsAddDataCommand;
    private ICommand filesToIgnoreAddDataCommand;
    private ICommand projectsToIgnoreAddDataCommand;
    private ICommand powerShellUpdateScriptsCommand;

    #endregion

    #region Constructor

    public CompilerSettingsViewModel()
    {
      compilerModel = SettingsProvider.CompilerSettingsModel;
      powerShellService = new PowerShellService();
    }

    #endregion

    #region Properties
    public CompilerSettingsModel CompilerModel
    {
      get
      {
        return compilerModel;
      }
      set
      {
        compilerModel = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CompilerModel"));
      }
    }

    public bool CanExecute
    {
      get
      {
        return true;
      }
    }

    public IEnumerable<ClangGeneralAdditionalIncludes> AdditionalIncludesItems
    {
      get
      {
        return Enum.GetValues(typeof(ClangGeneralAdditionalIncludes)).Cast<ClangGeneralAdditionalIncludes>();
      }
    }

    public SettingsTooltips Tooltip { get; } = new SettingsTooltips();

    #endregion

    #region Commands
    public ICommand CompileFlagsAddDataCommand
    {
      get => compileFlagsAddDataCommand ?? (compileFlagsAddDataCommand = new RelayCommand(() => UpdateCompileFlags(), () => CanExecute));
    }

    public ICommand FilesToIgnoreAddDataCommand
    {
      get => filesToIgnoreAddDataCommand ?? (filesToIgnoreAddDataCommand = new RelayCommand(() => UpdateFilesToIgnore(), () => CanExecute));
    }

    public ICommand ProjectsToIgnoreAddDataCommand
    {
      get => projectsToIgnoreAddDataCommand ?? (projectsToIgnoreAddDataCommand = new RelayCommand(() => UpdateProjectsToIgnore(), () => CanExecute));
    }

    public ICommand PowerShellUpdateScriptsCommand
    {
      get => powerShellUpdateScriptsCommand ?? (powerShellUpdateScriptsCommand = new RelayCommand(() => UpdateScriptsAsync().SafeFireAndForget(), () => CanExecute));
    }
    #endregion

    #region Methods
    private void UpdateCompileFlags()
    {
      compilerModel.CompileFlags = OpenContentDialog(compilerModel.CompileFlags);
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CompilerModel"));
    }

    private void UpdateFilesToIgnore()
    {
      compilerModel.FilesToIgnore = OpenContentDialog(compilerModel.FilesToIgnore);
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CompilerModel"));
    }

    private void UpdateProjectsToIgnore()
    {
      compilerModel.ProjectsToIgnore = OpenContentDialog(compilerModel.ProjectsToIgnore);
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CompilerModel"));
    }


    private async Task UpdateScriptsAsync()
    {
      await powerShellService.UpdateScriptsAsync();
    }
    #endregion;

  }
}
