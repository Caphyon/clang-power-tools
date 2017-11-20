using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ClangPowerTools
{
  public class RunningProcesses
  {
    #region Members

    private List<Process> mProcesses = new List<Process>();
    
    #endregion

    #region Public Methods

    public void Add(Process aProcess) => mProcesses.Add(aProcess);

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
        if(!process.HasExited)
          process.Kill();

      List<Process> processes = new List<Process>();
      processes.AddRange(Process.GetProcessesByName("clang++.exe"));
      processes.AddRange(Process.GetProcessesByName("clang-tidy.exe"));

      foreach (var process in processes)
        if(!process.HasExited)
          process.Kill();

      mProcesses.Clear();
    }

    #endregion

  }
}

