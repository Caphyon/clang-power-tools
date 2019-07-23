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
    #region Properties
    public string CompileFlags { get; set; }

    public string FilesToIgnore { get; set; }

    public string ProjectToIgnore { get; set; }

    public ClangGeneralAdditionalIncludes AdditionalIncludes { get; set; }

    public ClangGeneralAdditionalIncludes SelectedAdditionalInclude { get; set; } = ClangGeneralAdditionalIncludes.IncludeDirectories;

    public bool WarningsAsErrors { get; set; }

    public bool ContinueOnError { get; set; }

    public bool ClangCompileAfterMSCVCompile { get; set; }

    public bool VerboseMode { get; set; }
    #endregion
  }
}
