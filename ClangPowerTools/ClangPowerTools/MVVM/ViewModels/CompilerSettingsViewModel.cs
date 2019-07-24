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
    private const string GeneralSettingsFileName = "GeneralConfiguration.config";
    private string path = string.Empty;
    private ICommand addDataCommand;

    public event PropertyChangedEventHandler PropertyChanged;
    #endregion

    #region Constructors
    public CompilerSettingsViewModel()
    {
      SettingsPathBuilder settingsPathBuilder = new SettingsPathBuilder();
      path = settingsPathBuilder.GetPath(GeneralSettingsFileName);
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
      SettingsHandler.SaveToFile(path, compilerSettings);
    }


    public string CompileFlags
    {
      get
      {
        return string.IsNullOrWhiteSpace(compilerSettings.CompileFlags) ? DefaultOptions.kClangFlags : compilerSettings.CompileFlags;
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
        return compilerSettings.ProjectToIgnore;
      }
      set
      {
        compilerSettings.ProjectToIgnore = value;
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
        return "v" + compilerSettings.Version;
      }
      set
      {
        compilerSettings.ProjectToIgnore = value;
      }
    }

    #endregion


    #region Public Methods

    public void SaveSettingsToStorage()
    {
      compilerSettings.CompileFlags.Replace(" ", "").Trim(';');
      compilerSettings.FilesToIgnore.Replace(" ", "").Trim(';');
      compilerSettings.ProjectToIgnore.Replace(" ", "").Trim(';');

      SettingsHandler.SaveToFile(path, compilerSettings);
    }

    public void LoadSettingsFromStorage()
    {
      SettingsHandler.LoadFromFile(path, compilerSettings);

      //if (null == loadedConfig.CompileFlags || 0 == loadedConfig.CompileFlags.Count)
      //  CompileFlags = loadedConfig.CompileFlagsCollection;
      //else
      //  CompileFlags = string.Join(";", loadedConfig.CompileFlags);


      //if (null == loadedConfig.FilesToIgnore || 0 == loadedConfig.FilesToIgnore.Count)
      //{
      //  if (null == loadedConfig.FilesToIgnoreCollection)
      //  {
      //    FilesToIgnore = string.Empty;
      //  }
      //  else
      //  {
      //    FilesToIgnore = loadedConfig.FilesToIgnoreCollection;
      //  }
      //}
      //else
      //{
      //  FilesToIgnore = string.Join(";", loadedConfig.FilesToIgnore);
      //}

      //if (null == loadedConfig.ProjectsToIgnore || 0 == loadedConfig.ProjectsToIgnore.Count)
      //  ProjectsToIgnore = loadedConfig.ProjectsToIgnoreCollection ?? string.Empty;
      //else
      //  ProjectsToIgnore = string.Join(";", loadedConfig.ProjectsToIgnore);

      //AdditionalIncludes = null == loadedConfig.AdditionalIncludes ?
      //  ClangGeneralAdditionalIncludes.IncludeDirectories : loadedConfig.AdditionalIncludes;

      //WarningsAsErrors = loadedConfig.WarningsAsErrors;
      //ContinueOnError = loadedConfig.ContinueOnError;
      //ClangCompileAfterVsCompile = loadedConfig.ClangCompileAfterVsCompile;
      //VerboseMode = loadedConfig.VerboseMode;
      //Version = loadedConfig.Version;
    }

    //public void ResetSettings()
    //{
    //  SettingsHandler.CopySettingsProperties(SettingsProvider.GeneralSettings, new ClangGeneralOptionsView());
    //  SaveSettingsToStorage();
    //  LoadSettingsFromStorage();
    //}

    #endregion

  }
}
