using System;

namespace ClangPowerTools.Events
{
  public class FormatCommandEventArgs : EventArgs
  {
    public bool CanFormat { get; set; }
    public bool IgnoreFile { get; set; }
    public bool IgnoreExtension { get; set; }
    public string FileName { get; set; }

  }
}
