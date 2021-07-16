namespace ClangPowerTools
{
  public class FormatSettingsModel
  {
    public string FileExtensions { get; set; } = DefaultOptions.FileExtensions;

    public string FilesToIgnore { get; set; } = DefaultOptions.IgnoreFiles;

    public string AssumeFilename { get; set; } = string.Empty;

    public string CustomExecutable { get; set; } = string.Empty;

    public ClangFormatStyle Style { get; set; } = ClangFormatStyle.file;

    public ClangFormatFallbackStyle FallbackStyle { get; set; } = ClangFormatFallbackStyle.none;

    public bool FormatOnSave { get; set; } = false;
  }
}
