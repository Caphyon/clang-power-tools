using System.Collections.Generic;
using System.Diagnostics;

namespace ClangPowerTools
{
  public class RunningProcesses
  {
    #region Members

    private List<Process> mProcesses = new List<Process>();

    #endregion

    #region Public Methods

    public void Add(Process aProcess) => mProcesses.Add(aProcess);

    public void KillAll()
    {
      foreach (var process in mProcesses)
        process.Kill();
    }

    #endregion

  }
}

