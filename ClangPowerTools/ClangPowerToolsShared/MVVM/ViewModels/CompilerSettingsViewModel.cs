using ClangPowerTools.MVVM;
using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
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
    private ICommand addCptAliasCommand;

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
      get => powerShellUpdateScriptsCommand ?? (powerShellUpdateScriptsCommand = new RelayCommand(() => UpdateScripts(), () => CanExecute));
    }

    public ICommand AddCptAliasCommand
    {
      get => addCptAliasCommand ?? (addCptAliasCommand = new RelayCommand(() => AddCptAlias(), () => CanExecute));
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

    private void AddCptAlias()
    {
      string ScriptWindowsPowerShell =
        "if ((Test-Path -Path $profile -PathType Leaf) -eq $false) { New-Item -Path $profile -ItemType \"file\" -Force};" +
        $"Add-Content $profile ' Set-Alias -Name cpt -Value ''{PowerShellWrapper.GetClangBuildScriptPath()}'' ' ";
      if(PowerShellWrapper.Invoke(ScriptWindowsPowerShell, true))
      {
        MessageBox.Show("Cpt alias for Clang Power Tools script was added in your Powershell profile",
                                              "Clang Power Tools", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
      else 
      {
        MessageBox.Show("Sorry, we can't find Powershell 7 in PATH enviroment variables",
                                      "Clang Power Tools", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void UpdateScripts()
    {
      DialogResult dialogResult = MessageBox.Show("Do you want to update the PowerShell scripts?",
                                                  "Clang Power Tools", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
      if (dialogResult == DialogResult.Yes)
      {
        powerShellService.UpdateScriptsAsync().SafeFireAndForget();
      }
    }
    #endregion;

  }
}
