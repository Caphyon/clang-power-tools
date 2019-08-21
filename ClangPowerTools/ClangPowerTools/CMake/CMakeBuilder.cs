using ClangPowerTools.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerTools.CMake
{
  public class CMakeBuilder
  {

    public bool CreateBuildDirectory(string newDirName, out string dirPath)
    {
      dirPath = null;
      SolutionInfo.GetSolutionInfo(out string dir, out _, out _);
      if (Directory.Exists(dir) == false)
        return false;

      var buildDirectory = Path.Combine(dir, newDirName);
      if (Directory.Exists(buildDirectory))
        return true;

      Directory.CreateDirectory(buildDirectory);
      return true;
    }

    public void Build()
    {
      if (CreateBuildDirectory("build", out string dirPath) == false)
        return;

      var command = "cmake -DCMAKE_CONFIGURATION_TYPES=\"Debug; Release\" -DCMAKE_GENERATOR_PLATFORM=x64 -G \"Visual Studio 16 2019\" ..";

      using (System.Diagnostics.Process process = new System.Diagnostics.Process()
      {
        StartInfo = new System.Diagnostics.ProcessStartInfo()
        {
          WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
          WorkingDirectory = dirPath,
          FileName = "cmd.exe",
          Arguments = "/C " + command
        }
      })
      {
        process.Start();
        process.WaitForExit();
      }
    }

  }
}
