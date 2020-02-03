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
  public class SquiggleErrorTagger : ITagger<SquiggleErrorTag>
  {
    #region Members

    private readonly string squiggleType = "other error";

    private int line;
    private int column;

    private ITextBuffer SourceBuffer { get; set; }

    public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

    #endregion

    #region Constructor

    public SquiggleErrorTagger(ITextBuffer sourceBuffer)
    {
      SourceBuffer = sourceBuffer;
    }

    #endregion

    #region ITagger<HighlightWordTag> Implementation

    /// <summary>
    /// Find every instance of CurrentWord in the given span
    /// </summary>
    /// <param name="spans">A read-only span of text to be searched for instances of CurrentWord</param>
    public IEnumerable<ITagSpan<SquiggleErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans)
    {
      if (TaskErrorViewModel.FileErrorsPair == null || TaskErrorViewModel.FileErrorsPair.Count == 0)
        yield break;

      var dte = (DTE2)VsServiceProvider.GetService(typeof(DTE));
      var activeDocument = dte.ActiveDocument;

      if(TaskErrorViewModel.FileErrorsPair.ContainsKey(activeDocument.FullName) == false)
        yield break;

      foreach (var error in TaskErrorViewModel.FileErrorsPair[activeDocument.FullName])
      {
        var bufferLines = SourceBuffer.CurrentSnapshot.Lines.ToList();
        line = error.Line.ForceInRange(0, bufferLines.Count - 1);

        var currentLine = SourceBuffer.CurrentSnapshot.GetLineFromLineNumber(line);
        var currentLineText = currentLine.GetText().TrimEnd();

        if (string.IsNullOrWhiteSpace(currentLineText))
          continue;

        column = error.Column.ForceInRange(0, currentLineText.Length - 1);

        if (column - 1 >= 0 && column + 1 < currentLineText.Length)
        {
          if (currentLineText[column - 1] == ' ' && currentLineText[column] != ' ' && currentLineText[column + 1] == ' ')
          {
            yield return CreateTagSpan(column, 1, error.Text);
            continue;
          }
        }

        GetSquiggleValues(bufferLines, currentLineText, out int start, out int length);

        yield return CreateTagSpan(start, length, error.Text);
      }
    }

    private TagSpan<SquiggleErrorTag> CreateTagSpan(int start, int length, string tooltip )
    {
      var snapshotSpan = new SnapshotSpan(SourceBuffer.CurrentSnapshot, start, length);
      var squiggle = new SquiggleErrorTag(squiggleType, tooltip);

      return new TagSpan<SquiggleErrorTag>(snapshotSpan, squiggle);
    }

    private int LengthUntilGivenPosition(List<ITextSnapshotLine> lines)
    {
      var count = 0;
      for (var i = 0; i<line; ++i)
      {
        count += lines[i].GetText().Length;
      }

      return count + column + (line * 2) + 1;
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

    private void GetSquiggleValues(List<ITextSnapshotLine> lines, 
      string text, out int start, out int length)
    {
      start = LengthUntilGivenPosition(lines);
      start = start.ForceInRange(0, SourceBuffer.CurrentSnapshot.GetText().Length - 1);
      start = FindTheBeginning(text, start, column, out int stepsBack);
      
      length = FindLength(text, column - stepsBack + 1);
    }

    #endregion
  }


}
