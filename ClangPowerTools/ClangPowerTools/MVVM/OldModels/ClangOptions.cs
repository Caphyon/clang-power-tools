using System;
using System.Collections.Generic;

namespace ClangPowerTools
{
  public class ClangOptions
  {
    public string ProjectsToIgnore { get; set; } = string.Empty;

    public string ProjectsToIgnoreCollection { get; set; } = string.Empty;

    public string FilesToIgnore { get; set; } = string.Empty;

    public string FilesToIgnoreCollection { get; set; } = string.Empty;

    public bool Continue { get; set; } = false;

    public bool TreatWarningsAsErrors { get; set; } = false;

    public ClangGeneralAdditionalIncludes AdditionalIncludes { get; set; } = ClangGeneralAdditionalIncludes.IncludeDirectories;

    public bool VerboseMode { get; set; } = false;

    public string ClangFlags { get; set; } = string.Empty;

    public string ClangFlagsCollection { get; set; } = DefaultOptions.kClangFlags;

    public bool ClangCompileAfterVsCompile { get; set; } = false;

    public string Version { get; set; } = string.Empty;
  }
}
