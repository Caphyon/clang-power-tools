using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace ClangPowerTools.Squiggle
{

  /// <summary>
  /// This tagger will provide tags for every word in the buffer that
  /// matches the word currently under the cursor.
  /// </summary>
  public class SquiggleErrorTagger : ITagger<SquiggleErrorTag>
  {
    #region Members

    public IEnumerable<SquiggleModel> Squiggles { get; }

    public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

    #endregion

    #region Constructor

    public SquiggleErrorTagger(IEnumerable<SquiggleModel> squiggles)
    {
      Squiggles = squiggles;
    }

    #endregion

    #region ITagger<HighlightWordTag> Implementation

    /// <summary>
    /// Find every instance of CurrentWord in the given span
    /// </summary>
    /// <param name="spans">A read-only span of text to be searched for instances of CurrentWord</param>
    public IEnumerable<ITagSpan<SquiggleErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans)
    {
      if (Squiggles == null || Squiggles.Count() == 0)
        yield break;

      foreach (var errorSquiggle in Squiggles)
      {
        yield return new TagSpan<SquiggleErrorTag>(errorSquiggle.Snapshout, errorSquiggle.Squiggle);
      }
    }

    #endregion
  }


}
