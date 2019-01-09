using System;

namespace ClangPowerTools.Events
{
  public class ClangCommandMessageEventArgs : EventArgs
  {
    public string Message { get; private set; }

    public bool ClearFlag { get; set; }

    public ClangCommandMessageEventArgs(string aMessage, bool aClear)
    {
      Message = aMessage;
      ClearFlag = aClear;
    }
    
  }
}
