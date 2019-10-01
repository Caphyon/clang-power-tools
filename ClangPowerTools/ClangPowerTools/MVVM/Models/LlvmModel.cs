namespace ClangPowerTools
{
  public class LlvmModel
  {
    public string Version { get; set; } = string.Empty;

    public bool IsInstalled { get; set; } = false;

    public bool IsSelected { get; set; } = false;

    public int DownloadProgress { get; set; } = 0;

    public int MinProgress { get; set; } = 0;

    public int MaxProgress { get; set; } = 100;
  }
}
