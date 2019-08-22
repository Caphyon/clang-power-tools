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

    private CompilerSettingsModel compilerModel;
    private ICommand compileFlagsAddDataCommand;
    private ICommand filesToIgnoreAddDataCommand;
    private ICommand projectsToIgnoreAddDataCommand;
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
    #endregion

    #region Constructor
    public CompilerSettingsViewModel()
    {
      compilerModel = new CompilerSettingsModel();
      SettingsViewModelProvider.CompilerSettingsViewModel = this;
    }
    #endregion



    #region Commands
    public ICommand CompileFlagsAddDataCommand
    {
      get => compileFlagsAddDataCommand ?? (compileFlagsAddDataCommand = new RelayCommand(() => CompilerModel.CompileFlags = OpenContentDialog(CompilerModel.CompileFlags), () => CanExecute));
    }

    public ICommand FilesToIgnoreAddDataCommand
    {
      get => filesToIgnoreAddDataCommand ?? (filesToIgnoreAddDataCommand = new RelayCommand(() => CompilerModel.FilesToIgnore = OpenContentDialog(CompilerModel.FilesToIgnore), () => CanExecute));
    }

    public ICommand ProjectsToIgnoreAddDataCommand
    {
      get => projectsToIgnoreAddDataCommand ?? (projectsToIgnoreAddDataCommand = new RelayCommand(() => CompilerModel.ProjectsToIgnore = OpenContentDialog(CompilerModel.ProjectsToIgnore), () => CanExecute));
    }

    #endregion
  }
}
