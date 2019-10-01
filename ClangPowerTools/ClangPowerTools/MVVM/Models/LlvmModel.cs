namespace ClangPowerTools
{
  public class LlvmModel
  {
    public string Version { get; set; }

    public bool IsInstalled { get; set; }

    public bool IsSelected { get; set; }

    public int DownloadProgress { get; set; } = 0;

    public int MinProgress { get; set; } = 0;

    public int MaxProgress { get; set; } = 100;
  }
}
