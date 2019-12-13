using System;

namespace ClangPowerTools.Events
{
  public class FormatCommandEventArgs : EventArgs
  {
    public bool CanFormat { get; set; }
  }
}
