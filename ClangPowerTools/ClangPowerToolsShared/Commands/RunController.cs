using ClangPowerTools;
using ClangPowerTools.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClangPowerToolsShared.Commands
{
  public static class RunController
  {
    public static bool StopCommandActivated { get; set; } = false;
    public static RunningProcesses runningProcesses = new RunningProcesses();

    public static event EventHandler<CloseDataStreamingEventArgs> CloseDataStreamingEvent;

    public static void OnDataStreamClose(CloseDataStreamingEventArgs e)
    {
      CloseDataStreamingEvent?.Invoke(null, e);
    }
  }
}
