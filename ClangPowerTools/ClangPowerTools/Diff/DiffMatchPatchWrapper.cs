﻿using Compare.DiffMatchPatch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Media;

namespace ClangPowerTools
{
  public class DiffMatchPatchWrapper
  {
    #region Members

    private DiffMatchPatch diffMatchPatch;
    private List<Diff> diffs;

    // DiffMatchPatch defaults
    private readonly float diffTimeout = 1.0f;
    private readonly short diffEditCost = 4;
    private readonly float matchThreshold = 0.8f;
    private readonly int matchDistance = 1000;
    private readonly float patchDeleteThreshold = 0.5f;
    private readonly short patchMargin = 4;

    private const int LineWith = 1000;

    #endregion

    #region Constructors

    /// <summary>
    /// Initialize DiffMatchPatch and use the default values
    /// </summary>
    public DiffMatchPatchWrapper()
    {
      diffMatchPatch = new DiffMatchPatch();
    }

    /// <summary>
    /// Initialize DiffMatchPatch and overwrite the default values
    /// </summary>
    /// <param name="diffTimeout">Number of seconds to map a diff before giving up (0 for infinity).</param>
    /// <param name="diffEditCost">Cost of an empty edit operation in terms of edit characters.</param>
    /// <param name="matchThreshold">At what point is no match declared (0.0 = perfection, 1.0 = very loose).</param>
    /// <param name="matchDistance">How far to search for a match (0 = exact location, 1000+ = broad match). <br/>
    /// A match this many characters away from the expected location will add 1.0 to the score (0.0 is a perfect match)</param>
    /// <param name="patchDeleteThreshold">When deleting a large block of text (over ~64 characters), how close do the contents have to be to match the expected contents.<br/> 
    /// (0.0 = perfection, 1.0 = very loose).  Note that Match_Threshold controls how closely the end points of a delete need to match.</param>
    /// <param name="patchMargin">Chunk size for context length.</param>
    public DiffMatchPatchWrapper(float diffTimeout, short diffEditCost, float matchThreshold, int matchDistance, float patchDeleteThreshold, short patchMargin)
    {
      this.diffTimeout = diffTimeout;
      this.diffEditCost = diffEditCost;
      this.matchThreshold = matchThreshold;
      this.matchDistance = matchDistance;
      this.patchDeleteThreshold = patchDeleteThreshold;
      this.patchMargin = patchMargin;
    }

    #endregion

    #region Enum

    private enum LineChanges
    {
      NONE, HASCHANGES, NOCHANGES, NEWLINE
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// An array of differences is computed which describe the transformation of text1 into text2. 
    /// Each difference is an array of Diff objects. The first element specifies if it is an insertion (1), 
    /// a deletion (-1) or an equality (0). The second element specifies the affected text.
    /// </summary>
    public void Diff(string text1, string text2)
    {
      DiffMatchPatch diffMatchPatch = CreateDiffMatchPatch();
      diffs = diffMatchPatch.diff_main(text1, text2);
    }

    /// <summary>
    /// A semantic cleanup rewrites the diff, expanding it into a more human-readable format.
    /// </summary>
    /* For example, the diff of "mouse" and "sofas" is [(-1, "m"), (1, "s"), (0, "o"), (-1, "u"), (1, "fa"), (0, "s"), (-1, "e")]. 
    While this is the optimum diff, it is difficult for humans to understand. The above example would become: [(-1, "mouse"), (1, "sofas")]. If a diff is to be human-readable, it 
    should be passed to diff_cleanupSemantic */
    public void CleanupSemantic()
    {
      if (diffs == null) return;
      diffMatchPatch.diff_cleanupSemantic(diffs);
    }

    /// <summary>
    /// A cleanup rewrites the diff, it optimises the diff to be efficient for machine processing. 
    /// </summary>
    /* The results of both cleanup types are often the same. The efficiency cleanup is based on the observation that a diff made up of 
    large numbers of small diffs edits may take longer to process (in downstream applications) or take more capacity to store or transmit than a 
    smaller number of larger diffs. The diff_match_patch.Diff_EditCost property sets what the cost of handling a new edit is in terms of handling 
    extra characters in an existing edit. The default value is 4, which means if expanding the length of a diff by three characters can eliminate 
    one edit, then that optimisation will reduce the total costs. */
    public void CleanupEfficiency()
    {
      if (diffs == null) return;
      diffMatchPatch.diff_cleanupEfficiency(diffs);
    }

    /// <summary>
    /// Given a diff, measure its Levenshtein distance in terms of the number of inserted, deleted or substituted characters. 
    /// The minimum distance is 0 which means equality, the maximum distance is the length of the longer string.
    /// </summary>
    /// <returns>Number of changes or -1 if the Diff(text1, text2) was not run previously</returns>
    public int DiffLevenshtein()
    {
      if (diffs == null) return -1;
      return diffMatchPatch.diff_levenshtein(diffs);
    }

    /// <summary>
    /// Takes a diff array and returns a HTML sequence.
    /// </summary>
    /// <returns>Html page as string or string.Empty if the Diff(text1, text2) was not run previously</returns>
    public string DiffAsHtml()
    {
      if (diffs == null) return string.Empty;
      return diffMatchPatch.diff_prettyHtml(diffs);
    }

    /// <summary>
    /// Takes a diff array and returns a FlowDocument.
    /// </summary>
    /// <param name="editorInput">The input from the Format Editor</param>
    /// <param name="editorOutput">The output from the Format Editor</param>
    /// <returns></returns>
    public (FlowDocument, FlowDocument) DiffAsFlowDocuments(string editorInput, string editorOutput)
    {
      var paragraphInput = new Paragraph();
      var paragraphOutput = new Paragraph();
      var inputOperationPerLine = new List<(object, LineChanges)>();
      var outputOperationPerLine = new List<(object, LineChanges)>();

      DetectOperationPerLine(editorInput.Trim(), editorOutput.Trim(), inputOperationPerLine, outputOperationPerLine);
      CreateDiffParagraph(paragraphInput, inputOperationPerLine, Brushes.Orange);
      CreateDiffParagraph(paragraphOutput, outputOperationPerLine, Brushes.Yellow);

      return CreateFlowDocuments(paragraphInput, paragraphOutput);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Populate the inputOperationPerLine and outputOperationPerLine with the text and operation found on each line
    /// </summary>
    /// <param name="input"></param>
    /// <param name="output"></param>
    /// <param name="inputOperationPerLine"></param>
    /// <param name="outputOperationPerLine"></param>
    private void DetectOperationPerLine(string input, string output, List<(object, LineChanges)> inputOperationPerLine, List<(object, LineChanges)> outputOperationPerLine)
    {
      // Both inputLines and outputLines must be equal. The method is run on both
      // This is done my adding lines that contain just \r\n to the inputLines and outputLines
      List<string> inputLines = EqualizeDocumentLength(output, input);
      List<string> outputLines = EqualizeDocumentLength(input, output);

      if (inputLines.Count != outputLines.Count) return;

      for (int index = 0; index < outputLines.Count; index++)
      {
        var lineDiffs = GetDiff(inputLines[index], outputLines[index]);

        if (outputLines[index] == Environment.NewLine)
        {
          inputOperationPerLine.Add((inputLines[index], LineChanges.HASCHANGES));
          outputOperationPerLine.Add((outputLines[index], LineChanges.NEWLINE));
          continue;
        }
        else if (inputLines[index] == Environment.NewLine)
        {
          outputOperationPerLine.Add((outputLines[index], LineChanges.HASCHANGES));
          inputOperationPerLine.Add((inputLines[index], LineChanges.NEWLINE));
          continue;
        }

        var containsChanges = lineDiffs.Any(e => e.operation != Operation.EQUAL);
        if (containsChanges)
        {
          inputOperationPerLine.Add((inputLines[index], LineChanges.HASCHANGES));

          if (lineDiffs.Last().operation != Operation.EQUAL)
          {
            outputOperationPerLine.Add((outputLines[index], LineChanges.HASCHANGES));
          }
          else
          {
            outputOperationPerLine.Add((new List<Diff>(lineDiffs), LineChanges.HASCHANGES));
          }
        }
        else
        {
          inputOperationPerLine.Add((inputLines[index], LineChanges.NOCHANGES));
          outputOperationPerLine.Add((outputLines[index], LineChanges.NOCHANGES));
        }
      }
    }

    /// <summary>
    /// Run Diff and interpret operations to recreate the output as a list of lines.
    /// </summary>
    /// <param name="input">The original text</param>
    /// <param name="output">The modified text</param>
    /// <returns></returns>
    private List<string> EqualizeDocumentLength(string input, string output)
    {
      var diff = GetDiff(input, output);
      var lines = new List<string>();
      var totalNewLineFound = 0;

      for (int index = 0; index < diff.Count; index++)
      {
        var text = diff[index].text;
        var newLineFoundPerOperation = Regex.Matches(text, Environment.NewLine).Count;
        switch (diff[index].operation)
        {
          // Count when \r\n are deleted 
          case Operation.DELETE:
            if (newLineFoundPerOperation != 0)
            {
              totalNewLineFound += newLineFoundPerOperation;
            }
            break;
          case Operation.INSERT:
            // If a \r\n insert was found, remove one from the total of newLineFoundPerOperation 
            if (newLineFoundPerOperation != 0)
            {
              totalNewLineFound -= newLineFoundPerOperation;
            }

            // If no \r\n was found,  append the text from the operation to the previous line
            if (newLineFoundPerOperation == 0)
            {
              lines[lines.Count - 1] += text;
              break;
            }

            // If one \r\n was found, append the text before \r\n to the the previous line and 
            // the text after add it to a new line
            if (newLineFoundPerOperation == 1)
            {
              lines[lines.Count - 1] += text.SubstringBefore(Environment.NewLine);
              lines.Add(text.SubstringAfter(Environment.NewLine));
              break;
            }

            // Multiple \r\n were found, append the text before the first \r\n to the the previous line 
            // and split the remaing text based on \r\n to new lines
            var insertLines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();
            lines[lines.Count - 1] += insertLines[0];
            insertLines.RemoveAt(0);

            lines.AddRange(insertLines);
            break;
          case Operation.EQUAL:
            var equalLines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();

            // If one \r\n was found, append the text before \r\n to the the previous line and 
            // the text after add it to a new line
            if (lines.Count > 0 && equalLines.Count >= 1 && equalLines.Count > newLineFoundPerOperation)
            {
              lines[lines.Count - 1] += equalLines.First();
              equalLines.RemoveAt(0);
            }

            // Insert empty lines to equalize the length of the input and output
            if (newLineFoundPerOperation > 1)
            {
              for (int i = 0; i < totalNewLineFound; i++)
              {
                lines.Add(Environment.NewLine);
              }
              totalNewLineFound = 0;
            }
            lines.AddRange(equalLines);
            break;
          default:
            break;
        }
      }
      return lines;
    }

    private void CreateDiffParagraph(Paragraph paragraph, List<(object, LineChanges)> operationLines, Brush lineDiffColor)
    {
      for (int i = 0; i < operationLines.Count; i++)
      {
        AddLineNumberToParagraphLine(paragraph, i + 1, operationLines.Count, 2);

        var run = new Run();
        switch (operationLines[i].Item2)
        {
          case LineChanges.NOCHANGES:
            run.Text = (string)operationLines[i].Item1;
            paragraph.Inlines.Add(run);
            paragraph.Inlines.Add(Environment.NewLine);
            break;
          case LineChanges.HASCHANGES:
            if (operationLines[i].Item1 is List<Diff> list)
            {
              ColorTextDependingOnOperation(paragraph, list);
            }
            else
            {
              run.Text = AddPadding((string)operationLines[i].Item1, LineWith, false);
              run.Background = lineDiffColor;
              paragraph.Inlines.Add(run);
              paragraph.Inlines.Add(Environment.NewLine);
            }
            break;
          case LineChanges.NEWLINE:
            run.Text = AddPadding((string)operationLines[i].Item1, LineWith, true);
            run.Background = (Brush)new BrushConverter().ConvertFrom("#D3D3D3");
            paragraph.Inlines.Add(run);
            break;
        }
      }
    }

    private (FlowDocument, FlowDocument) CreateFlowDocuments(Paragraph paragraphInput, Paragraph paragraphOutput)
    {
      var diffInput = new FlowDocument();
      var diffOutput = new FlowDocument();
      diffInput.Blocks.Add(paragraphInput);
      diffOutput.Blocks.Add(paragraphOutput);
      return (diffInput, diffOutput);
    }

    private string AddPadding(string text, int targetPadding, bool isNewLine)
    {
      var paddingCount = targetPadding - text.Length;
      if (isNewLine)
      {
        return new string(' ', paddingCount) + text;
      }
      return text + new string(' ', paddingCount);
    }

    private List<Diff> GetDiff(string inputLine, string outputLine)
    {
      diffMatchPatch = new DiffMatchPatch();
      List<Diff> diff = diffMatchPatch.diff_main(inputLine, outputLine);
      diffMatchPatch.diff_cleanupSemantic(diff);
      return diff;
    }

    private void ColorTextDependingOnOperation(Paragraph paragraph, List<Diff> localdiffs)
    {
      foreach (Diff aDiff in localdiffs)
      {
        var text = aDiff.text;
        var run = new Run();

        switch (aDiff.operation)
        {
          case Operation.INSERT:
            run.Text = text;
            run.Background = Brushes.LightGreen;
            paragraph.Inlines.Add(run);
            break;
          case Operation.DELETE:
            run.Text = text;
            run.Background = Brushes.LightPink;
            paragraph.Inlines.Add(run);
            break;
          case Operation.EQUAL:
            run.Text = text;
            paragraph.Inlines.Add(run);
            break;
        }
      }
      paragraph.Inlines.Add(Environment.NewLine);
    }

    private void AddLineNumberToParagraphLine(Paragraph paragraph, int currentLineNumber, int numberOfLines, int paddingLeft)
    {
      // A digit occupies two '  ' not ' ' in a Run
      int numberOfSpaces = (LengthOfNumber(numberOfLines) - LengthOfNumber(currentLineNumber) + paddingLeft) * 2;
      var lineNumber = string.Concat(new string(' ', numberOfSpaces), (currentLineNumber).ToString(), " ");
      var lineNumberRun = new Run(lineNumber)
      {
        Background = (Brush)new BrushConverter().ConvertFrom("#D3D3D3")
      };
      paragraph.Inlines.Add(lineNumberRun);
    }

    private int LengthOfNumber(int numberOfLines)
    {
      return (int)Math.Floor(Math.Log10(numberOfLines) + 1);
    }

    private DiffMatchPatch CreateDiffMatchPatch()
    {
      return new DiffMatchPatch
      {
        Diff_Timeout = diffTimeout,
        Diff_EditCost = diffEditCost,
        Match_Threshold = matchThreshold,
        Match_Distance = matchDistance,
        Patch_DeleteThreshold = patchDeleteThreshold,
        Patch_Margin = patchMargin
      };
    }

    #endregion
  }
}
