using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;

namespace ClangPowerTools.Squiggle
{

  public class HighlightWordTag : ErrorTag
  {
    public HighlightWordTag() : base("error", "This is a tooltip error") { }
  }

  /// <summary>
  /// This tagger will provide tags for every word in the buffer that
  /// matches the word currently under the cursor.
  /// </summary>
  public class HighlightWordTagger : ITagger<HighlightWordTag>
  {
    private ITextView View { get; set; }

    private ITextBuffer SourceBuffer { get; set; }

    private ITextSearchService TextSearchService { get; set; }

    private ITextStructureNavigator TextStructureNavigator { get; set; }

    private object updateLock = new object();

    // The current set of words to highlight
    private NormalizedSnapshotSpanCollection WordSpans { get; set; }
    private SnapshotSpan? CurrentWord { get; set; }

    // The current request, from the last cursor movement or view render
    private SnapshotPoint RequestedPoint { get; set; }

    public HighlightWordTagger(ITextView view, ITextBuffer sourceBuffer, ITextSearchService textSearchService,
                               ITextStructureNavigator textStructureNavigator)
    {
      View = view;
      SourceBuffer = sourceBuffer;
      TextSearchService = textSearchService;
      TextStructureNavigator = textStructureNavigator;

      WordSpans = new NormalizedSnapshotSpanCollection();
      CurrentWord = null;
    }


    #region ITagger<HighlightWordTag> Members

    /// <summary>
    /// Find every instance of CurrentWord in the given span
    /// </summary>
    /// <param name="spans">A read-only span of text to be searched for instances of CurrentWord</param>
    public IEnumerable<ITagSpan<HighlightWordTag>> GetTags(NormalizedSnapshotSpanCollection spans)
    {
      var column = 1;
      var highlightLine = 12;
      var characterCount = 0;
      var lines = SourceBuffer.CurrentSnapshot.Lines.ToList();

      if (highlightLine > lines.Count)
        yield break;


      --highlightLine;

      --column;

      if (column < 0)
        column = 0;

      if (highlightLine < 0)
        highlightLine = 0;

      var currentLine = SourceBuffer.CurrentSnapshot.GetLineFromLineNumber(highlightLine);

      var text = currentLine.GetText().TrimEnd();



      if (column >= text.Length)
        column = text.Length - 1;


      if (column - 1 >= 0 && column + 1 < text.Length)
      {
        if (text[column - 1] == ' ' && text[column] != ' ' && text[column + 1] == ' ')
        {
          var snapshotSpanForOneElement = new SnapshotSpan(SourceBuffer.CurrentSnapshot, column, 1);
          yield return new TagSpan<HighlightWordTag>(snapshotSpanForOneElement, new HighlightWordTag());
          yield break;
        }
      }


      for (int i = 0; i < highlightLine; i++)
        characterCount += lines[i].GetText().Length;


      if (string.IsNullOrWhiteSpace(text))
        yield break;

      if (column >= text.Length)
        yield break;

      
      var start = characterCount + column + highlightLine + highlightLine + 1;

      if (start < 0)
        start = 0;

      if (start >= SourceBuffer.CurrentSnapshot.GetText().Length)
      {
        start = SourceBuffer.CurrentSnapshot.GetText().Length - 1;
      }

      var iterations = 0;
      for (int i = column; i >= 0; --i)
      {
        if (text[i] == ' ' || text[i] == '\n' || text[i] == '\r')
        {
          break;
        }

        ++iterations;
        --start;
      }


      var length = 0;
      for (int i = column - iterations + 1; i < text.Length; ++i)
      {
        if (text[i] == ' ')
          break;

        ++length;
      }

      //var startPoint = characterCount + highlightLine + 1;
      var startPoint = start;

      if (startPoint >= SourceBuffer.CurrentSnapshot.GetText().Length)
      {
        startPoint = SourceBuffer.CurrentSnapshot.GetText().Length - 1;
      }

      //var highlightLength = currentLine.Length + 1;
      var highlightLength = length;

      var snapshotSpan = new SnapshotSpan(SourceBuffer.CurrentSnapshot, startPoint, highlightLength);

      yield return new TagSpan<HighlightWordTag>(snapshotSpan, new HighlightWordTag());
    }

    public bool ok = false;

    public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

    #endregion
  }

}
