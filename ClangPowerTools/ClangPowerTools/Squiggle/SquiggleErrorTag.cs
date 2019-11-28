using Microsoft.VisualStudio.Text.Tagging;

namespace ClangPowerTools.Squiggle
{
  public class SquiggleErrorTag : ErrorTag
  {
    public SquiggleErrorTag(string type, string tooltip) : base(type, tooltip) { }
  }
}
