using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerTools
{
  public class CompilerSettingsViewModel : INotifyPropertyChanged
  {
    #region Members
    private CompilerSettingsModel compilerSettings = new CompilerSettingsModel();
    public event PropertyChangedEventHandler PropertyChanged;
    #endregion


    #region Properties
    public string CompileFlags
    {
      get { return compilerSettings.CompileFlags; }
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

    public string ProjectToIgnore
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
      get { return compilerSettings.SelectedAdditionalInclude; }
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

    public bool ContinueOnError { get; set; }

    public bool ClangCompileAfterMSCVCompile { get; set; }

    public bool VerboseMode { get; set; }

    #endregion
  }
}
