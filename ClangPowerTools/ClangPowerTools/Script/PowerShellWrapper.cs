using ClangPowerTools.Commands;
using System;
using System.Diagnostics;
using System.Linq;

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

    public static void Invoke(string aScript, RunningProcesses runningProcesses)
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

         if (BackgroundTidyCommand.Running == false)
        {
          process.EnableRaisingEvents = true;
          process.ErrorDataReceived += DataErrorHandler;
          process.OutputDataReceived += DataHandler;
          process.Exited += ExitedHandler;
          process.Disposed += ExitedHandler;
        }

        runningProcesses.Add(process, BackgroundTidyCommand.Running);

        process.Start();

        process.BeginErrorReadLine();
        process.BeginOutputReadLine();

        process.WaitForExit();
      }
      catch (Exception e)
      {
        if (BackgroundTidyCommand.Running == false)
        {
          process.EnableRaisingEvents = false;
          process.ErrorDataReceived -= DataErrorHandler;
          process.OutputDataReceived -= DataHandler;
          process.Exited -= ExitedHandler;
          process.Disposed -= ExitedHandler;
        }

        process.Close();

        throw e;
      }
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

