using System;

namespace ClangPowerTools.Events
{
  public class FormatCommandEventArgs : EventArgs
  {
    public bool CanFormat { get; set; } = false;
    public bool IgnoreFile { get; set; } = false;
    public bool IgnoreExtension { get; set; } = false;
    public bool Clear{ get; set; } = false;
    public bool FormatConfigFound { get; set; } = true;
    public string FileName { get; set; }
  }
}
