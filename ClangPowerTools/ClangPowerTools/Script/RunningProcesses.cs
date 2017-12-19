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

    private const string processeName = "powershell";
    private List<Process> mProcesses = new List<Process>();

    #endregion

    #region Public Methods

    public void Add(Process aProcess)
    {
      mProcesses.Add(aProcess);
    }

    public void KillById(int aId)
    {
      var procees = mProcesses.FirstOrDefault(p => p.Id == aId);
      if (null == procees)
        return;
      procees.Kill();
    }

    public void KillAll()
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

    //private void KillProcessAndChildren(string processName)
    //{
    //  MessageBox.Show(processeName);
    //  var retVal = Process.GetProcesses().Where(p => p.ProcessName.Contains(processName)).ToList();
    //  foreach (var proc in retVal)
    //  {
    //    if (proc.HasExited)
    //      continue;
    //    proc.Kill();
    //  }

    //}

    private void KillProcessAndChildren(int aPid)
    {
      // Cannot close 'system idle process'.
      if (0 == aPid)
        return;

      ManagementObjectSearcher searcher = new ManagementObjectSearcher
        ("Select * From Win64_Process Where ParentProcessID=" + aPid);
      ManagementObjectCollection moc = searcher.Get();

      foreach (ManagementObject mo in moc)
        KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));

      try
      {
        Process proc = Process.GetProcessById(aPid);
        if (!proc.HasExited)
        {
          proc.Kill();
        }
      }
      catch (ArgumentException e)
      {
        MessageBox.Show(e.Message);
        // Process already exited.
      }
    }

  }

  #endregion

}


