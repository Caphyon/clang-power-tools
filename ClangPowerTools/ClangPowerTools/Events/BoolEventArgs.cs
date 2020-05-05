using System;

namespace ClangPowerTools.Events
{
  public class BoolEventArgs : EventArgs
  {
    public bool Value { get; set; }

    public BoolEventArgs(bool value) => Value = value;

  }
}
