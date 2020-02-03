using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace ClangPowerTools.Squiggle
{
  /// <summary>
  /// Export a <see cref="IViewTaggerProvider"/>
  /// </summary>
  [Export(typeof(IViewTaggerProvider))]
  [ContentType("text")]
  [TagType(typeof(SquiggleErrorTag))]
  public class SquiggleErrorTaggerProvider : IViewTaggerProvider
  {
    #region ITaggerProvider Members

    [Import]
    internal ITextSearchService TextSearchService { get; set; }

    [Import]
    internal ITextStructureNavigatorSelectorService TextStructureNavigatorSelector { get; set; }

    /// <summary>
    /// This method is called by VS to generate the tagger
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="textView"> The text view we are creating a tagger for</param>
    /// <param name="buffer"> The buffer that the tagger will examine for instances of the current word</param>
    /// <returns> Returns a HighlightWordTagger instance</returns>
    public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
    {
      if (textView.TextBuffer != buffer)
      {
        return null;
      }

      SquigglesFactory squigglesFactory = new SquigglesFactory(buffer);
      IEnumerable<SquiggleModel> squiggles = squigglesFactory.Create();
      
      return new SquiggleErrorTagger(squiggles) as ITagger<T>;
    }

    #endregion
  }
}
