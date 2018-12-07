using Microsoft.VisualStudio.Shell.Interop;

namespace ClangPowerTools.Events
{
  public class VsHierarchyDetectedEventArgs
  {
    public IVsHierarchy Hierarchy { get; set; }


    public VsHierarchyDetectedEventArgs(IVsHierarchy aHierarchy)
    {
      Hierarchy = aHierarchy;
    }
  }
}
