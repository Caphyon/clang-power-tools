﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;

namespace ClangPowerTools
{
  public class RunningProcesses
  {
    #region Members

    private readonly static List<Process> commandProcesses = new List<Process>();
    private readonly static List<Process> backgroundCommandProcesses = new List<Process>();
    private readonly bool backgroundProcess = false;

    #endregion


    #region Constructor 

    public RunningProcesses(bool background = false)
    {
      backgroundProcess = background;
    }

    #endregion


    #region Public Methods

    public void Add(Process aProcess)
    {
      if (backgroundProcess)
        backgroundCommandProcesses.Add(aProcess);
      else
        commandProcesses.Add(aProcess);
    }

    public void Add(Process aProcess, bool background = false)
    {
      if (background)
        backgroundCommandProcesses.Add(aProcess);
      else
        commandProcesses.Add(aProcess);
    }

    public bool Exists(bool backgroundRunners)
    {
      return backgroundRunners ?
        backgroundCommandProcesses.Count != 0 : commandProcesses.Count != 0;
    }

    public void Kill(bool background)
    {
      var processes = GetProcesses(background);

      foreach (var process in processes)
      {
        if (process.HasExited || process.Responding == false)
          continue;

        KillProcessAndChildren(process.Id);
      }

      Clear(processes);
    }

    public void KillById(int aId)
    {
      var procees = commandProcesses.FirstOrDefault(p => p.Id == aId);
      if (null == procees)
        return;
      procees.Kill();
    }

    #endregion


    #region Private Methods

    private List<Process> GetProcesses(bool background)
    {
      return background ? backgroundCommandProcesses : commandProcesses;
    }

    private void Clear(List<Process> processes)
    {
      processes.ForEach(process => process.Close());
      processes.Clear();
    }

    private static void KillProcessAndChildren(int aPid)
    {
      // Cannot close 'system idle process'
      if (aPid == 0)
        return;

      Process proc = new Process();
      try
      {
        ManagementObjectSearcher searcher = new ManagementObjectSearcher
          ("Select * From Win32_Process Where ParentProcessID=" + aPid);
        ManagementObjectCollection moc = searcher.Get();

        foreach (ManagementObject mo in moc)
          KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));

        proc = Process.GetProcessById(aPid);
        proc.Kill();
      }
      catch (Exception)
      {
        // The process has already exited.
        proc.Close();
      }
    }

    #endregion

  }
}


