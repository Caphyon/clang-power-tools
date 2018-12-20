using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;

namespace ClangPowerTools
{
  public class RunningProcesses
  {
    #region Members

    private List<Process> mProcesses = new List<Process>();

    #endregion


    #region Public Methods

    public void Add(Process aProcess) => mProcesses.Add(aProcess);

    public void Kill()
    {
      foreach (var process in mProcesses)
      {
        if (process.HasExited)
          continue;

        KillProcessAndChildren(process.Id);
        process.Dispose();
      }
      mProcesses.Clear();
    }

    public void KillById(int aId)
    {
      var procees = mProcesses.FirstOrDefault(p => p.Id == aId);
      if (null == procees)
        return;
      procees.Kill();
    }

    #endregion

    #region Private Methods

    private static void KillProcessAndChildren(int aPid)
    {
      // Cannot close 'system idle process'
      if (aPid == 0)
        return;

      try
      {
        ManagementObjectSearcher searcher = new ManagementObjectSearcher
          ("Select * From Win32_Process Where ParentProcessID=" + aPid);
        ManagementObjectCollection moc = searcher.Get();

        foreach (ManagementObject mo in moc)
          KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));

        Process proc = Process.GetProcessById(aPid);
        proc.Kill();
      }
      catch (ArgumentException) { } // The process has already exited.
    }

    #endregion

  }
}


