using System.IO;

namespace ClangPowerTools.Helpers
{
  public static class FileSystem
  {
    public static void CreateDirectory(string path)
    {
      if (Directory.Exists(path) == false) Directory.CreateDirectory(path);
    }

    public static void DeleteDirectory(string path)
    {
      if (Directory.Exists(path)) Directory.Delete(path, true);
    }

    public static void DeleteFile(string path)
    {
      if (File.Exists(path)) File.Delete(path);
    }

    public static bool DoesFileExist(string path, string fileName)
    {
      var filePath = string.Concat(path, "\\", fileName);
      return File.Exists(filePath);
    }

    public static string CreateFullFileName(string path, string fileName)
    {
      return string.Concat(path, "\\", fileName);
    }
  }
}
