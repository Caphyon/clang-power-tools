using System.IO;

namespace ClangPowerTools.Helpers
{
  public class FileSystem
  {
    public void CreateDirectory(string path)
    {
      if (Directory.Exists(path) == false) Directory.CreateDirectory(path);
    }

    public void DeleteDirectory(string path)
    {
      if (Directory.Exists(path)) Directory.Delete(path, true);
    }

    public void DeleteFile(string path)
    {
      if (File.Exists(path)) File.Delete(path);
    }
  }
}
