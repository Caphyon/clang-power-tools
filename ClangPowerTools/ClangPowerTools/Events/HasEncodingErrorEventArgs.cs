using ClangPowerTools.Output;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerTools.Events
{
  public class HasEncodingErrorEventArgs
  {
    public OutputContentModel Model { get; set; }

    public HasEncodingErrorEventArgs(OutputContentModel model)
    {
      Model = model;
    }
  }
}
