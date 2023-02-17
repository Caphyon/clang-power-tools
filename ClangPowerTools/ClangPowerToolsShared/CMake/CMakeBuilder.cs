using ClangPowerTools.Helpers;
using System.IO;

namespace ClangPowerTools.CMake
{
  public class CMakeBuilder
  {
    #region Members

    private readonly string directoryName = "CPTCMakeBuild";

    #endregion

    #region Public Methods

    /// <summary>
    /// Create the build directory("CPTCMakeBuild") for the current CMake project
    /// </summary>
    /// <param name="newDirName">The name of the created directory</param>
    /// <param name="dirPath">The full path of the created directory</param>
    /// <returns>True if the directory is created successfully. False otherwise.</returns>
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

    /// <summary>
    /// Build the current CMake project. The process will generate one .sln file and posible more .vcxproj files.
    /// </summary>
    public void Build()
    {
      if (CreateBuildDirectory(directoryName, out string dirPath) == false)
        return;

      var command = "cmake -G \"Visual Studio 16 2019\" ..";

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

    /// <summary>
    /// Delete from the disk the CMake build directory and all its content.
    /// </summary>
    public void ClearBuildCashe()
    {
      CreateBuildDirectory(directoryName, out string dirPath);
      if (string.IsNullOrWhiteSpace(dirPath))
        return;

      Directory.Delete(dirPath, true);
    }

    #endregion

  }
}
