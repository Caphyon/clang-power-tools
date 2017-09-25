using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
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

    public void Invoke(List<Tuple<IItem, IVsHierarchy>> aItems, ScriptBuiler aScriptBuilder)
    {
      foreach (var itm in aItems)
      {
        Process p = new Process();
        try
        {
          string script = aScriptBuilder.GetScript(itm.Item1, itm.Item1.GetName());
          p.StartInfo = new ProcessStartInfo()
          {
            FileName = $"{Environment.SystemDirectory}\\{ScriptConstants.kPowerShellPath}",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            UseShellExecute = false,
            Arguments = script
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
    }

    #endregion

  }
}
