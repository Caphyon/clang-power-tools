using Microsoft.VisualStudio.Shell.Interop;
using System;

namespace ClangPowerTools.Output
{
  /// <summary>
  /// Contains all the necessary VS Output Window elements
  /// 
  public class OutputWindowModel
  {
    /// <summary>
    /// Output window instance
    /// </summary>
    public IVsOutputWindow VsOutputWindow { get; set; }

    /// <summary>
    /// The output window pane to display messages
    /// </summary>
    public IVsOutputWindowPane Pane { get; set; }

    /// <summary>
    /// Standard guid for the output window pane 
    /// </summary>
    public Guid PaneGuid { get; } = new Guid("AB9F45E4-2001-4197-BAF5-4B165222AF29");
  }
}
