using System;

namespace ClangPowerTools.Events
{
  public class ClangCommandEventArgs : EventArgs
  {
    public string Message { get; private set; }

    public bool ClearFlag { get; set; }

    public ClangCommandEventArgs(string aMessage, bool aClear)
    {
      Message = aMessage;
      ClearFlag = aClear;
    }
    
  }
}
