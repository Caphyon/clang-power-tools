namespace ClangPowerTools
{
  public class TidySettingsModel
  {
    public string HeaderFilter { get; set; } = DefaultOptions.HeaderFilter;

    public ClangTidyChecksFrom UseChecksFrom { get; set; } = ClangTidyChecksFrom.Checks;

    public string Checks { get; set; } = string.Empty;

    public string CustomExecutable { get; set; } = string.Empty;

    public bool FormatAfterTidy { get; set; } = false;

    public bool TidyOnSave { get; set; } = false;
  }
}
