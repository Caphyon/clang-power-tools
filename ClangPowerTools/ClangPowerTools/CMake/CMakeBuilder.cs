using ClangPowerTools.Helpers;
using System.IO;

namespace ClangPowerTools.CMake
{
  public class CMakeBuilder
  {
    private readonly string directoryName = "build";

    public bool CreateBuildDirectory(string newDirName, out string dirPath)
    {
      dirPath = null;
      SolutionInfo.GetSolutionInfo(out string dir, out _, out _);
      if (Directory.Exists(dir) == false)
        return false;

      dirPath = Path.Combine(dir, newDirName);
      if (Directory.Exists(dirPath))
        return true;

      Directory.CreateDirectory(dirPath);
      return true;
    }

    public void Build()
    {
      if (CreateBuildDirectory(directoryName, out string dirPath) == false)
        return;

      var command = "cmake -DCMAKE_CONFIGURATION_TYPES=\"Debug; Release\" -DCMAKE_GENERATOR_PLATFORM=x64 -G \"Visual Studio 16 2019\" ..";

      using (System.Diagnostics.Process process = new System.Diagnostics.Process()
      {
        StartInfo = new System.Diagnostics.ProcessStartInfo()
        {
          CreateNoWindow = true,
          WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
          WorkingDirectory = dirPath,
          FileName = "cmd.exe",
          Arguments = "/c " + command
        }
      })
      {
        process.Start();
        process.WaitForExit();
      }
    }

    public void ClearBuildCashe()
    {
      CreateBuildDirectory(directoryName, out string dirPath);
      if (string.IsNullOrWhiteSpace(dirPath))
        return;

      Directory.Delete(dirPath, true);
    }

  }
}
