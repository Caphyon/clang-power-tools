namespace ClangPowerTools.Events
{
  public class JsonFilePathArgs
  {
    public string FilePath { get; set; }

    public JsonFilePathArgs(string path)
    {
      FilePath = path;
    }
  }
}
