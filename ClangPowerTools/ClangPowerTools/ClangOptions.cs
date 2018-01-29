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

    public List<string> ProjectsToIgnore { get; set; } = new List<string>();

    public List<string> FilesToIgnore { get; set; } = new List<string>();

    public bool Continue { get; set; }

    public bool TreatWarningsAsErrors { get; set; }

    public string AdditionalIncludes { get; set; }

    public bool VerboseMode { get; set; }

    public List<string> ClangFlags { get; set; } = new List<string>();

    public string Version { get; set; }

    #endregion


  }
}
