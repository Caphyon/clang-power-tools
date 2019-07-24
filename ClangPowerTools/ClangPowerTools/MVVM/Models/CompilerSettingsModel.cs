using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerTools
{
  public class CompilerSettingsModel
  {
    public string CompileFlags { get; set; } = string.Empty;

    public string FilesToIgnore { get; set; } = string.Empty;

    public string ProjectToIgnore { get; set; } = string.Empty;

    public ClangGeneralAdditionalIncludes AdditionalIncludes { get; set; } = ClangGeneralAdditionalIncludes.IncludeDirectories;

    public bool WarningsAsErrors { get; set; } = false;

    public bool ContinueOnError { get; set; } = false;

    public bool ClangCompileAfterMSCVCompile { get; set; } = false;

    public bool VerboseMode { get; set; } = false;

    public string Version { get; set; } = SettingsProvider.GeneralSettings.Version;
  }
}
