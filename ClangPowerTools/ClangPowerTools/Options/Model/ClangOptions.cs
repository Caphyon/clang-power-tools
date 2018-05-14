using System;
using System.Collections.Generic;

namespace ClangPowerTools
{
  [Serializable]
  public class ClangOptions
  {
    #region Members

    private List<string> mClangFlags = new List<string>();

    #endregion

    #region Properties

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public List<string> ProjectsToIgnore { get; set; } = new List<string>();

    public string ProjectsToIgnoreCollection { get; set; }


    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public List<string> FilesToIgnore { get; set; } = new List<string>();

    public string FilesToIgnoreCollection { get; set; }


    public bool Continue { get; set; }

    public bool TreatWarningsAsErrors { get; set; }

    public ClangGeneralAdditionalIncludes? AdditionalIncludes { get; set; }

    public bool VerboseMode { get; set; }


    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public List<string> ClangFlags { get; set; } = new List<string>();

    public string ClangFlagsCollection { get; set; }


    public bool ClangCompileAfterVsCompile { get; set; }

    public string Version { get; set; }

    #endregion

  }
}
