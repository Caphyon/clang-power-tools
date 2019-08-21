using ClangPowerTools.MVVM.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class CompilerSettingsViewModel : CommonSettingsFunctionality, INotifyPropertyChanged
  {
    #region Members
    public event PropertyChangedEventHandler PropertyChanged;

    private CompilerSettingsModel compilerSettingsModel;
    private ICommand compileFlagsAddDataCommand;
    private ICommand filesToIgnoreAddDataCommand;
    private ICommand projectsToIgnoreAddDataCommand;
    #endregion

    #region Constructor
    public CompilerSettingsViewModel()
    {
      compilerSettingsModel = SettingsModelProvider.CompilerSettings;
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
        return string.IsNullOrWhiteSpace(compilerSettingsModel.CompileFlags) ? DefaultOptions.ClangFlags : compilerSettingsModel.CompileFlags;
      }
      set
      {
        compilerSettingsModel.CompileFlags = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CompileFlags"));
      }
    }

    public string FilesToIgnore
    {
      get
      {
        return compilerSettingsModel.FilesToIgnore;
      }
      set
      {
        compilerSettingsModel.FilesToIgnore = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FilesToIgnore"));
      }
    }

    public string ProjectsToIgnore
    {
      get
      {
        return compilerSettingsModel.ProjectsToIgnore;
      }
      set
      {
        compilerSettingsModel.ProjectsToIgnore = value;
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
      get { return compilerSettingsModel.AdditionalIncludes; }
      set
      {
        compilerSettingsModel.AdditionalIncludes = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedAdditionalInclude"));
      }
    }

    public bool WarningsAsErrors
    {
      get
      {
        return compilerSettingsModel.WarningsAsErrors;
      }

      set
      {
        compilerSettingsModel.WarningsAsErrors = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("WarningsAsErrors"));
      }
    }

    public bool ContinueOnError
    {
      get
      {
        return compilerSettingsModel.ContinueOnError;
      }
      set
      {
        compilerSettingsModel.ContinueOnError = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ContinueOnErrorOnError"));
      }
    }

    public bool ClangCompileAfterMSCVCompile
    {
      get
      { return compilerSettingsModel.ClangCompileAfterMSCVCompile; }
      set
      {
        compilerSettingsModel.ClangCompileAfterMSCVCompile = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ClangCompileAfterMSCVCompile"));
      }
    }

    public bool VerboseMode
    {
      get
      {
        return compilerSettingsModel.VerboseMode;
      }
      set
      {
        compilerSettingsModel.VerboseMode = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("VerboseMode"));
      }
    }

    public string Version
    {
      get
      {
        return compilerSettingsModel.Version;
      }
    }

    #endregion
  }
}
