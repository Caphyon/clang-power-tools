using System;
using System.IO;
using System.Windows.Forms;

namespace ClangPowerTools.Helpers
{
  public static class FileSystem
  {
    #region Properties

    public static string ConfigClangFormatFileName { get; } = ".clang-format";

    public static string ConfigClangTidyFileName { get; } = ".clang-tidy";

    #endregion


    #region Methods

    public static bool SearchAllTopDirectories(string filePath, string searchedFileName)
    {
      while (string.IsNullOrEmpty(filePath) == false)
      {
        if (FileSystem.DoesFileExist(filePath, searchedFileName))
          return true;

        var index = filePath.LastIndexOf("\\");
        if (index > 0)
          filePath = filePath.Remove(index);
        else
          return false;
      }

      return false;
    }

    public static void CreateDirectory(string path)
    {
      if (Directory.Exists(path) == false) Directory.CreateDirectory(path);
    }

    public static void DeleteDirectory(string path)
    {
      if (Directory.Exists(path))
      {
        try
        {
          Directory.Delete(path, true);
        }
        catch (Exception e)
        {
          MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
      }
    }

    public static void DeleteFile(string path)
    {
      if (File.Exists(path))
      {
        try
        {
          File.Delete(path);
        }
        catch (Exception e)
        {
          MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
      }
    }

    public static bool DoesFileExist(string path, string fileName)
    {
      var filePath = string.Concat(path, "\\", fileName);
      return File.Exists(filePath);
    }

    public static void WriteContentToFile(string path, string content)
    {
      using FileStream fs = new FileStream(path, FileMode.Create);
      using StreamWriter sw = new StreamWriter(fs);
      sw.Write(content);
    }

    public static string CreateFullFileName(string path, string fileName)
    {
      return string.Concat(path, "\\", fileName);
    }

    #endregion

  }
}
