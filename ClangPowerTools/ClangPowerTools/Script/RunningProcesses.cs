using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Windows.Forms;

namespace ClangPowerTools
{
  public class RunningProcesses
  {
    #region Members

    private readonly List<Process> processes = new List<Process>();

    #endregion

    #region Public Methods

    public void Add(Process aProcess) => processes.Add(aProcess);

    public bool Running() => processes.Count > 0;

    public void Kill()
    {
      if (processes.Count <= 0)
        return;

      processes.ForEach(process => process.Dispose());
      processes.Clear();
    }

    public void KillById(int aId)
    {
      var procees = processes.FirstOrDefault(p => p.Id == aId);
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
      catch (ArgumentException e)
      {
        MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      } // The process has already exited.
    }

    #endregion

  }
}


