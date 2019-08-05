using ClangPowerTools.MVVM.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class CompilerSettingsViewModel : INotifyPropertyChanged
  {
    #region Members
    private CompilerSettingsModel compilerSettings = new CompilerSettingsModel();
    private ICommand addDataCommand;

    public event PropertyChangedEventHandler PropertyChanged;
    #endregion

    #region Constructors
    public CompilerSettingsViewModel()
    {
      CPTSettings.CompilerSettings = compilerSettings;
    }
    #endregion


    #region Properties
    public ICommand AddDataCommand
    {
      get => addDataCommand ?? (addDataCommand = new RelayCommand(() => OpenDataDialog(), () => CanExecute));
    }

    public bool CanExecute
    {
      get
      {
        return true;
      }
    }

    public void OpenDataDialog()
    {
      MessageBox.Show("Hello, world!");
      CPTSettings cPTSettings = new CPTSettings();
      cPTSettings.CheckOldSettings();
    }


    public string CompileFlags
    {
      get
      {
        return string.IsNullOrWhiteSpace(compilerSettings.CompileFlags) ? DefaultOptions.ClangFlags : compilerSettings.CompileFlags;
      }
      set
      {
        compilerSettings.CompileFlags = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CompileFlags"));
      }
    }

    public string FilesToIgnore
    {
      get
      {
        return compilerSettings.FilesToIgnore;
      }
      set
      {
        compilerSettings.FilesToIgnore = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FilesToIgnore"));
      }
    }

    public string ProjectsToIgnore
    {
      get
      {
        return compilerSettings.ProjectsToIgnore;
      }
      set
      {
        compilerSettings.ProjectsToIgnore = value;
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
      get { return compilerSettings.AdditionalIncludes; }
      set
      {
        compilerSettings.AdditionalIncludes = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedAdditionalInclude"));
      }
    }

    public bool WarningsAsErrors
    {
      get
      {
        return compilerSettings.WarningsAsErrors;
      }

      set
      {
        compilerSettings.WarningsAsErrors = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("WarningsAsErrors"));
      }
    }

    public bool ContinueOnError
    {
      get
      {
        return compilerSettings.ContinueOnError;
      }
      set
      {
        compilerSettings.ContinueOnError = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ContinueOnErrorOnError"));
      }
    }

    public bool ClangCompileAfterMSCVCompile
    {
      get
      { return compilerSettings.ClangCompileAfterMSCVCompile; }
      set
      {
        compilerSettings.ClangCompileAfterMSCVCompile = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ClangCompileAfterMSCVCompile"));
      }
    }

    public bool VerboseMode
    {
      get
      {
        return compilerSettings.VerboseMode;
      }
      set
      {
        compilerSettings.VerboseMode = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("VerboseMode"));
      }
    }

    public string Version
    {
      get
      {
        return compilerSettings.Version;
      }
    }

    #endregion
  }
}
