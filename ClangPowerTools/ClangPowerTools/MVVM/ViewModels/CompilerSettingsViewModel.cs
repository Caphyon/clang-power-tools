using ClangPowerTools.MVVM.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class CompilerSettingsViewModel : CommonSettingsFunctionality, INotifyPropertyChanged
  {
    #region Members
    public event PropertyChangedEventHandler PropertyChanged;

    private ICommand compileFlagsAddDataCommand;
    private ICommand filesToIgnoreAddDataCommand;
    private ICommand projectsToIgnoreAddDataCommand;
    #endregion

    #region Constructor
    public CompilerSettingsViewModel()
    {
      SettingsViewModelProvider.CompilerSettingsViewModel = this;
    }
    #endregion

    #region Commands
    public ICommand CompileFlagsAddDataCommand
    {
      get => compileFlagsAddDataCommand ?? (compileFlagsAddDataCommand = new RelayCommand(() => CompileFlags = OpenContentDialog(CompileFlags), () => CanExecute));
    }

    public ICommand FilesToIgnoreAddDataCommand
    {
      get => filesToIgnoreAddDataCommand ?? (filesToIgnoreAddDataCommand = new RelayCommand(() => FilesToIgnore = OpenContentDialog(FilesToIgnore), () => CanExecute));
    }

    public ICommand ProjectsToIgnoreAddDataCommand
    {
      get => projectsToIgnoreAddDataCommand ?? (projectsToIgnoreAddDataCommand = new RelayCommand(() => ProjectsToIgnore = OpenContentDialog(ProjectsToIgnore), () => CanExecute));
    }

    #endregion


    #region Properties
    public bool CanExecute
    {
      get
      {
        return true;
      }
    }

    public string CompileFlags
    {
      get
      {
        return string.IsNullOrWhiteSpace(SettingsModelProvider.CompilerSettings.CompileFlags) ? DefaultOptions.ClangFlags : SettingsModelProvider.CompilerSettings.CompileFlags;
      }
      set
      {
        SettingsModelProvider.CompilerSettings.CompileFlags = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CompileFlags"));
      }
    }

    public string FilesToIgnore
    {
      get
      {
        return SettingsModelProvider.CompilerSettings.FilesToIgnore;
      }
      set
      {
        SettingsModelProvider.CompilerSettings.FilesToIgnore = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FilesToIgnore"));
      }
    }

    public string ProjectsToIgnore
    {
      get
      {
        return SettingsModelProvider.CompilerSettings.ProjectsToIgnore;
      }
      set
      {
        SettingsModelProvider.CompilerSettings.ProjectsToIgnore = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ProjectToIgnore"));
      }
    }

    public IEnumerable<ClangGeneralAdditionalIncludes> AdditionalIncludes
    {
      get
      {
        return Enum.GetValues(typeof(ClangGeneralAdditionalIncludes)).Cast<ClangGeneralAdditionalIncludes>();
      }
    }

    public ClangGeneralAdditionalIncludes SelectedAdditionalInclude
    {
      get { return SettingsModelProvider.CompilerSettings.AdditionalIncludes; }
      set
      {
        SettingsModelProvider.CompilerSettings.AdditionalIncludes = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedAdditionalInclude"));
      }
    }

    public bool WarningsAsErrors
    {
      get
      {
        return SettingsModelProvider.CompilerSettings.WarningsAsErrors;
      }

      set
      {
        SettingsModelProvider.CompilerSettings.WarningsAsErrors = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("WarningsAsErrors"));
      }
    }

    public bool ContinueOnError
    {
      get
      {
        return SettingsModelProvider.CompilerSettings.ContinueOnError;
      }
      set
      {
        SettingsModelProvider.CompilerSettings.ContinueOnError = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ContinueOnErrorOnError"));
      }
    }

    public bool ClangCompileAfterMSCVCompile
    {
      get
      { return SettingsModelProvider.CompilerSettings.ClangCompileAfterMSCVCompile; }
      set
      {
        SettingsModelProvider.CompilerSettings.ClangCompileAfterMSCVCompile = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ClangCompileAfterMSCVCompile"));
      }
    }

    public bool VerboseMode
    {
      get
      {
        return SettingsModelProvider.CompilerSettings.VerboseMode;
      }
      set
      {
        SettingsModelProvider.CompilerSettings.VerboseMode = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("VerboseMode"));
      }
    }

    public string Version
    {
      get
      {
        return SettingsModelProvider.CompilerSettings.Version;
      }
    }

    #endregion
  }
}
