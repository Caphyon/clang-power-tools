using System;
using System.Diagnostics;

namespace ClangPowerTools
{
  public class PowerShellWrapper
  {
    #region Properties

    public DataReceivedEventHandler DataErrorHandler { get; set; }
    public DataReceivedEventHandler DataHandler { get; set; }

    #endregion

    #region Public Methods

    public Process Invoke(string aScript, RunningProcesses aRunningProcesses)
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
          Arguments = aScript
        };

        process.ErrorDataReceived += DataErrorHandler;
        process.OutputDataReceived += DataHandler;

        aRunningProcesses.Add(process);

        process.Start();
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();
        process.WaitForExit();
      }
      finally
      {
        process.ErrorDataReceived -= DataErrorHandler;
        process.ErrorDataReceived -= DataHandler;
      }
      return process;
    }

    #endregion

  }
}

