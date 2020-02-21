using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace ClangPowerTools
{
  public static class PowerShellWrapper
  {
    #region Properties

    public static DataReceivedEventHandler DataErrorHandler { get; set; }
    public static DataReceivedEventHandler DataHandler { get; set; }
    public static EventHandler ExitedHandler { get; set; }

    #endregion

    #region Public Methods

    public static Process Invoke(string aScript, RunningProcesses aRunningProcesses)
    {
      Process process = new Process();
      try
      {
        process.StartInfo = new ProcessStartInfo()
        {
          FileName = $"{Environment.SystemDirectory}\\{ScriptConstants.kPowerShellPath}",
          RedirectStandardError = true,
          RedirectStandardOutput = true,
          CreateNoWindow = true,
          UseShellExecute = false,
          Arguments = aScript,
        };
        process.StartInfo.EnvironmentVariables["Path"] = CreatePathEnvironmentVariable();

        process.EnableRaisingEvents = true;
        process.ErrorDataReceived += DataErrorHandler;
        process.OutputDataReceived += DataHandler;
        process.Exited += ExitedHandler;
        process.Disposed += ExitedHandler;

        aRunningProcesses.Add(process);

        process.Start();
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();
        process.WaitForExit();
      }
      catch (Exception e)
      {
        MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      finally
      {
        process.ErrorDataReceived -= DataErrorHandler;
        process.OutputDataReceived -= DataHandler;
        process.Exited -= ExitedHandler;
        process.EnableRaisingEvents = false;
        process.Close();
      }
      return process;
    }

    #endregion


    #region Private Methods
    private static string CreatePathEnvironmentVariable()
    {
      var path = Environment.GetEnvironmentVariable("Path");
      var settingsProvider = new SettingsProvider();
      var llvmVersion = settingsProvider.GetCompilerSettingsModel().LlvmVersion;

      if (string.IsNullOrEmpty(llvmVersion)) return path;

      var paths = path.Split(';').ToList();
      paths.RemoveAt(paths.Count - 1);
      paths.RemoveAll(ContainsLlvm);
      paths.Add(GetUsedLlvmVersionPath(llvmVersion));

      return String.Join(";", paths);
    }


    private static string GetUsedLlvmVersionPath(string llvmVersion)
    {
      var settingsPathBuilder = new SettingsPathBuilder();      
      return settingsPathBuilder.GetLlvmBinPath(llvmVersion);
    }

    private static bool ContainsLlvm(string input)
    {
      return input.ToLower().Contains("llvm");
    }
    #endregion

  }
}

