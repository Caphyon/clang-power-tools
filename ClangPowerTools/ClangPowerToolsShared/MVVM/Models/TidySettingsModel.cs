namespace ClangPowerTools
{
  public class TidySettingsModel
  {
    public string HeaderFilter { get; set; } = DefaultOptions.HeaderFilter;

    public ClangTidyUseChecksFrom UseChecksFrom { get; set; } = ClangTidyUseChecksFrom.PredefinedChecks;

    public string PredefinedChecks { get; set; } = string.Empty;

    public string CustomChecks { get; set; } = string.Empty;

    public string CustomExecutable { get; set; } = string.Empty;

    public bool DetectClangTidyFile { get; set; } = true;

    public bool FormatAfterTidy { get; set; } = false;
    public bool TidyOnSave { get; set; } = false;
    public bool ApplyTidyFix { get; set; } = false;
  }
}
