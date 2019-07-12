using System.Collections.Generic;

namespace ClangPowerTools.Events
{
  public class ErrorDetectedEventArgs
  {
    public IEnumerable<TaskErrorModel> ErrorList { get; set; }

    public bool IsErrorWindowFocused { get; set; }
    public ErrorDetectedEventArgs(IEnumerable<TaskErrorModel> aErrorList, bool isErrorWindowFocused)
    {
      ErrorList = aErrorList;
      IsErrorWindowFocused = isErrorWindowFocused;
    }
  }
}
