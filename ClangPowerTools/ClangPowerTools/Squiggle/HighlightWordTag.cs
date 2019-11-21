using Microsoft.VisualStudio.Text.Tagging;

namespace ClangPowerTools.Squiggle
{
  public class HighlightWordTag : ErrorTag
  {
    public HighlightWordTag() : base("error", "This is a tooltip error") { }
  }
}
