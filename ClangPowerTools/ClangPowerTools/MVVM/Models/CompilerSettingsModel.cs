namespace ClangPowerTools
{
  public class CompilerSettingsModel
  {
    public string CompileFlags { get; set; } = DefaultOptions.ClangFlags;

    public string FilesToIgnore { get; set; } = string.Empty;

    public string ProjectsToIgnore { get; set; } = string.Empty;

    public ClangGeneralAdditionalIncludes AdditionalIncludes { get; set; } = ClangGeneralAdditionalIncludes.IncludeDirectories;

    public bool WarningsAsErrors { get; set; } = false;

    public bool ContinueOnError { get; set; } = false;

    public bool ClangCompileAfterMSCVCompile { get; set; } = false;

    public bool VerboseMode { get; set; } = false;

  }
}
