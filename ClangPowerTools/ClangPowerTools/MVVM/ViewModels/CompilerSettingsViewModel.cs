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

    private ICommand compileFlagsAddDataCommand;
    private ICommand filesToIgnoreAddDataCommand;
    private ICommand projectsToIgnoreAddDataCommand;
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

    public bool CanExecute
    {
      get
      {
        return true;
      }
    }
    #endregion


    #region Properties
    public string CompileFlags
    {
      get
      {
        return string.IsNullOrWhiteSpace(SettingsModelHandler.CompilerSettings.CompileFlags) ? DefaultOptions.ClangFlags : SettingsModelHandler.CompilerSettings.CompileFlags;
      }
      set
      {
        SettingsModelHandler.CompilerSettings.CompileFlags = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CompileFlags"));
      }
    }

    public string FilesToIgnore
    {
      get
      {
        return SettingsModelHandler.CompilerSettings.FilesToIgnore;
      }
      set
      {
        SettingsModelHandler.CompilerSettings.FilesToIgnore = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FilesToIgnore"));
      }
    }

    public string ProjectsToIgnore
    {
      get
      {
        return SettingsModelHandler.CompilerSettings.ProjectsToIgnore;
      }
      set
      {
        SettingsModelHandler.CompilerSettings.ProjectsToIgnore = value;
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
      get { return SettingsModelHandler.CompilerSettings.AdditionalIncludes; }
      set
      {
        SettingsModelHandler.CompilerSettings.AdditionalIncludes = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedAdditionalInclude"));
      }
    }

    public bool WarningsAsErrors
    {
      get
      {
        return SettingsModelHandler.CompilerSettings.WarningsAsErrors;
      }

      set
      {
        SettingsModelHandler.CompilerSettings.WarningsAsErrors = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("WarningsAsErrors"));
      }
    }

    public bool ContinueOnError
    {
      get
      {
        return SettingsModelHandler.CompilerSettings.ContinueOnError;
      }
      set
      {
        SettingsModelHandler.CompilerSettings.ContinueOnError = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ContinueOnErrorOnError"));
      }
    }

    public bool ClangCompileAfterMSCVCompile
    {
      get
      { return SettingsModelHandler.CompilerSettings.ClangCompileAfterMSCVCompile; }
      set
      {
        SettingsModelHandler.CompilerSettings.ClangCompileAfterMSCVCompile = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ClangCompileAfterMSCVCompile"));
      }
    }

    public bool VerboseMode
    {
      get
      {
        return SettingsModelHandler.CompilerSettings.VerboseMode;
      }
      set
      {
        SettingsModelHandler.CompilerSettings.VerboseMode = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("VerboseMode"));
      }
    }

    public string Version
    {
      get
      {
        return SettingsModelHandler.CompilerSettings.Version;
      }
    }

    #endregion
  }
}
