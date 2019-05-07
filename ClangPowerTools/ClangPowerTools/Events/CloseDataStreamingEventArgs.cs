using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerTools.Events
{
  public class CloseDataStreamingEventArgs
  {
    public bool IsStopped { get; set; }

    public CloseDataStreamingEventArgs(bool isStopped)
    {
      IsStopped = isStopped;
    }
  }
}
