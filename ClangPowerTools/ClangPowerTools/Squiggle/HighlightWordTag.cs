using Microsoft.VisualStudio.Text.Tagging;

namespace ClangPowerTools.Squiggle
{
  public class HighlightWordTag : ErrorTag
  {
    public HighlightWordTag(string type, string tooltip) : base(type, tooltip) { }
  }
}
