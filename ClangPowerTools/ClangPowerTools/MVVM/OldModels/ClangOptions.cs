using System;
using System.Collections.Generic;

namespace ClangPowerTools
{
  public class ClangOptions
  {
    public string ClangFlagsCollection { get; set; }

    public string ProjectsToIgnoreCollection { get; set; }

    public string FilesToIgnoreCollection { get; set; }


    public bool Continue { get; set; }

    public bool TreatWarningsAsErrors { get; set; }

    public ClangGeneralAdditionalIncludes? AdditionalIncludes { get; set; }

    public bool VerboseMode { get; set; }

    public bool ClangCompileAfterVsCompile { get; set; }

    public string Version { get; set; }

  }
}
