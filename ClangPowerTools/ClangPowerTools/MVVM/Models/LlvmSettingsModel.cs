namespace ClangPowerTools
{
  public class LlvmSettingsModel
  {
    public int DownloadProgress { get; set; } = 0;

    public int MinProgress { get; set; } = 0;

    public int MaxProgress { get; set; } = 100;

    public string Version { get; set; } = string.Empty;
  }
}

