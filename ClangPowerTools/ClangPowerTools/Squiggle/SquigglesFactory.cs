using ClangPowerTools.Error;
using ClangPowerTools.Services;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Text;
using System.Collections.Generic;
using System.Linq;

namespace ClangPowerTools.Squiggle
{
  public class SquigglesFactory
  {
    #region Members

    private readonly string squiggleType = "other error";

    private ITextBuffer SourceBuffer { get; set; }
    private int line;
    private int column;

    private const int rangeArea = 100;

    public static List<SquiggleModel> Squiggles { get; set; } = new List<SquiggleModel>();

    #endregion

    #region Constuctor

    public SquigglesFactory(ITextBuffer sourceBuffer)
    {
      SourceBuffer = sourceBuffer;
    }

    #endregion

    #region Public Methods

    public void Create()
    {
      if (TaskErrorViewModel.FileErrorsPair == null || TaskErrorViewModel.FileErrorsPair.Count == 0)
        return;

      var dte = (DTE2)VsServiceProvider.GetService(typeof(DTE));
      var activeDocument = dte.ActiveDocument;

      if (activeDocument == null)
        return;

      if (dte.ActiveWindow.Selection is TextSelection == false)
        return;

      TextSelection textSelection = (TextSelection) dte.ActiveWindow.Selection;
      if (textSelection == null)
        return;

      var currentLineNumber = textSelection.CurrentLine;

      if (TaskErrorViewModel.FileErrorsPair.ContainsKey(activeDocument.FullName) == false)
        return;

      foreach (var error in TaskErrorViewModel.FileErrorsPair[activeDocument.FullName])
      {
        //var min = currentLineNumber - rangeArea <= 1 ? 1 : currentLineNumber - rangeArea;
        //var max = currentLineNumber + rangeArea >= SourceBuffer.CurrentSnapshot.GetText().Length ?
        //  SourceBuffer.CurrentSnapshot.GetText().Length - 1 : currentLineNumber + rangeArea;

        //if (error.Line < min || error.Line > max)
        //  continue;

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
            Squiggles.Add(CreateTagSpan(column, 1, error.Text));
            continue;
          }
        }

        GetSquiggleValues(bufferLines, currentLineText, out int start, out int length);

        Squiggles.Add(CreateTagSpan(start, length, error.Text));
      }
    }

    #endregion

    #region Private Methods

    private SquiggleModel CreateTagSpan(int start, int length, string tooltip)
    {
      var snapshotSpan = new SnapshotSpan(SourceBuffer.CurrentSnapshot, start, length);
      var squiggle = new SquiggleErrorTag(squiggleType, tooltip);

      return new SquiggleModel()
      {
        Snapshout = snapshotSpan,
        Squiggle = squiggle
      };
    }

    private int LengthUntilGivenPosition(List<ITextSnapshotLine> lines)
    {
      var count = 0;
      for (var i = 0; i < line; ++i)
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
