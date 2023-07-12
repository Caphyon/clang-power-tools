using System.ComponentModel;
using System.Windows.Forms;

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

    public ClangVerbosityLevel VerbosityLevel { get; set; } = ClangVerbosityLevel.Warning;

    public bool Powershell7 
    { 
      get { return powershell7; } 
      set
      {
        if(value && string.IsNullOrEmpty(PowerShellWrapper.GetFilePathFromEnviromentVar(ScriptConstants.kPwsh)))
        {
          MessageBox.Show("Sorry, we can't find Powershell 7 in PATH enviroment variables",
                              "Clang Power Tools", MessageBoxButtons.OK, MessageBoxIcon.Error);
          return;
        }
        powershell7 = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Powershell7"));

      }
    }

    public bool ShowErrorList { get; set; } = true;

    public bool ShowOutputWindow { get; set; } = true;

    public bool ShowSquiggles { get; set; } = false;

    public int CpuLimit { get; set; } = 100;
  }
}
