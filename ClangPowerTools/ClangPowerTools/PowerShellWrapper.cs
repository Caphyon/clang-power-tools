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

    public void Invoke(string aScript)
    {
      Process p = new Process();
      try
      {
        p.StartInfo = new ProcessStartInfo()
        {
          FileName = $"{Environment.SystemDirectory}\\{ScriptConstants.kPowerShellPath}",
          RedirectStandardError = true,
          RedirectStandardOutput = true,
          CreateNoWindow = true,
          UseShellExecute = false,
          Arguments = aScript
        };

        p.ErrorDataReceived += DataErrorHandler;
        p.OutputDataReceived += DataHandler;
        p.Start();
        p.BeginErrorReadLine();
        p.BeginOutputReadLine();
        p.WaitForExit();
      }
      finally
      {
        p.ErrorDataReceived -= DataErrorHandler;
        p.ErrorDataReceived -= DataHandler;
      }
    }

    #endregion

  }
}

