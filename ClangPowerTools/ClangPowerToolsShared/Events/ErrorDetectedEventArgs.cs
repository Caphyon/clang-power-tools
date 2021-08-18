using System.Collections.Generic;

namespace ClangPowerTools.Events
{
  public class ErrorDetectedEventArgs
  {
    public IEnumerable<TaskErrorModel> ErrorList { get; set; }

    public ErrorDetectedEventArgs(IEnumerable<TaskErrorModel> aErrorList)
    {
      ErrorList = aErrorList;
    }
  }
}
