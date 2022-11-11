using System.ComponentModel;

namespace ClangPowerTools
{
  public class CompilerSettingsModel : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    private bool powershell7 = false;
    public string CompileFlags { get; set; } = DefaultOptions.ClangFlags;

    public string FilesToIgnore { get; set; } = string.Empty;

    public string ProjectsToIgnore { get; set; } = string.Empty;

    public ClangGeneralAdditionalIncludes AdditionalIncludes { get; set; } = ClangGeneralAdditionalIncludes.IncludeDirectories;

    public bool WarningsAsErrors { get; set; } = false;

    public bool ContinueOnError { get; set; } = false;

    public bool ClangAfterMSVC { get; set; } = false;

    public bool VerboseMode { get; set; } = false;
    public bool Powershell7 
    { 
      get { return powershell7; } 
      set
      {
        powershell7 = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Powershell7"));

      }
    }

    public bool ShowErrorList { get; set; } = true;

    public bool ShowOutputWindow { get; set; } = true;

    public bool ShowSquiggles { get; set; } = false;
  }
}
