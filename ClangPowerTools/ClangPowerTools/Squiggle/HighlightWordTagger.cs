using System;
using System.Collections.Generic;
using System.Linq;
using ClangPowerTools.Error;
using ClangPowerTools.Services;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace ClangPowerTools.Squiggle
{

  /// <summary>
  /// This tagger will provide tags for every word in the buffer that
  /// matches the word currently under the cursor.
  /// </summary>
  public class HighlightWordTagger : ITagger<HighlightWordTag>
  {
    #region Members

    private ITextBuffer SourceBuffer { get; set; }

    private Document activeDocument;

    public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

    #endregion

    #region Constructor

    public HighlightWordTagger(ITextBuffer sourceBuffer)
    {
      SourceBuffer = sourceBuffer;
      var dte = (DTE2)VsServiceProvider.GetService(typeof(DTE));
      activeDocument = dte.ActiveDocument;
    }

    #endregion

    #region ITagger<HighlightWordTag> Implementation

    /// <summary>
    /// Find every instance of CurrentWord in the given span
    /// </summary>
    /// <param name="spans">A read-only span of text to be searched for instances of CurrentWord</param>
    public IEnumerable<ITagSpan<HighlightWordTag>> GetTags(NormalizedSnapshotSpanCollection spans)
    {
      if (TaskErrorViewModel.Errors == null || TaskErrorViewModel.Errors.Count == 0)
        yield break;

      var errors = TaskErrorViewModel.Errors.Where(err => err.Document != activeDocument.Name);

      foreach (var error in errors)
      {
        var bufferLines = SourceBuffer.CurrentSnapshot.Lines.ToList();

        error.Line = error.Line.ForceInRange(0, bufferLines.Count - 1);

        var currentLine = SourceBuffer.CurrentSnapshot.GetLineFromLineNumber(error.Line);
        var text = currentLine.GetText().TrimEnd();

        if (string.IsNullOrWhiteSpace(text))
          continue;

        error.Column = error.Column.ForceInRange(0, text.Length - 1);

        if (error.Column - 1 >= 0 && error.Column + 1 < text.Length)
        {
          if (text[error.Column - 1] == ' ' && text[error.Column] != ' ' && text[error.Column + 1] == ' ')
          {
            var snapshotSpanForOneElement = new SnapshotSpan(SourceBuffer.CurrentSnapshot, error.Column, 1);
            var squiggleTag = new HighlightWordTag("error", error.Text);
            SquiggleViewModel.Squiggles.Add(squiggleTag);
            yield return new TagSpan<HighlightWordTag>(snapshotSpanForOneElement, squiggleTag);
            continue;
          }
        }

        GetSquiggleValues(error, bufferLines, text, error.Column, out int start, out int length);

        var snapshotSpan = new SnapshotSpan(SourceBuffer.CurrentSnapshot, start, length);

        var squiggle = new HighlightWordTag("error", error.Text);
        SquiggleViewModel.Squiggles.Add(squiggle);

        yield return new TagSpan<HighlightWordTag>(snapshotSpan, squiggle);
      }

    }

    private int LengthUntilGivenPosition(TaskErrorModel error, List<ITextSnapshotLine> lines)
    {
      var count = 0;
      for (var i = 0; i<error.Line; ++i)
      {
        count += lines[i].GetText().Length;
      }

      return count + error.Column + (error.Line * 2) + 1;
    }

    private int FindTheBeginning(string text, int start, int iterationValue, out int stepsBack)
    {
      stepsBack = 0;
      for (int i = iterationValue; i >= 0; --i)
      {
        if (text[i] == ' ' || text[i] == '\n' || text[i] == '\r')
        {
          break;
        }

        ++stepsBack;
        --start;
      }

      return start;
    }

    private int FindLength(string text, int start)
    {
      var length = 0;
      for (int i = start; i < text.Length; ++i)
      {
        if (text[i] == ' ')
          break;

        ++length;
      }

      return length;
    }

    private void GetSquiggleValues(TaskErrorModel error, List<ITextSnapshotLine> lines, 
      string text, int column, out int start, out int length)
    {
      start = LengthUntilGivenPosition(error, lines);
      start = start.ForceInRange(0, SourceBuffer.CurrentSnapshot.GetText().Length - 1);
      start = FindTheBeginning(text, start, column, out int stepsBack);
      
      length = FindLength(text, error.Column - stepsBack + 1);
    }

    #endregion
  }


}
