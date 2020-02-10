using System;
using System.Windows.Documents;
using System.Windows.Media;

namespace ClangPowerTools
{
  /// <summary>
  /// Class for text manipulation operations
  /// </summary>
  public class TextManipulation
  {
    /// <summary>
    /// Is manipulating a specific string inside of a TextPointer Range
    /// </summary>
    /// <param name="startPointer">Starting point where to look</param>
    /// <param name="endPointer">Endpoint where to look</param>
    /// <param name="keyword">This is the string you want to manipulate</param>
    /// <param name="foreground">The new foreground</param>
    /// <param name="newString">The New String (if you want to replace, can be null)</param>
    public static void FromTextPointer(TextPointer startPointer, TextPointer endPointer, string keyword, Brush foreground)
    {
      if (startPointer == null) throw new ArgumentNullException(nameof(startPointer));
      if (endPointer == null) throw new ArgumentNullException(nameof(endPointer));
      if (string.IsNullOrEmpty(keyword)) throw new ArgumentNullException(keyword);

      TextRange text = new TextRange(startPointer, endPointer);
      TextPointer position = text.Start.GetInsertionPosition(LogicalDirection.Forward);
      while (position != null)
      {
        string textInRun = position.GetTextInRun(LogicalDirection.Forward);

        if (string.IsNullOrWhiteSpace(textInRun))
        {
          position = position.GetNextContextPosition(LogicalDirection.Forward);
          continue;
        }

        int index = textInRun.IndexOf(keyword);

        if (index != -1 && CheckForSpaceAfterKeyword(textInRun, keyword, index))
        {
          TextRange selection = CreateSelection(position, keyword.Length, index);

          if (keyword == " " || text.Text == string.Empty)
          {
            selection.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
          }
          else
          {
            selection.ApplyPropertyValue(TextElement.ForegroundProperty, foreground);
          }
        }

        position = position.GetNextContextPosition(LogicalDirection.Forward);
      }
    }

    private static bool CheckForSpaceAfterKeyword(string text, string keyword, int index)
    {
      int keywordLength = keyword.Length;

      if (index + keywordLength > text.Length - 1) return false;
      var characterToCheck = text[index + keywordLength];

      if (characterToCheck == ' ' || characterToCheck == '/') return true;
   
      return false;
    }

    private static TextRange CreateSelection(TextPointer position, int keywordLength, int index)
    {
      TextPointer selectionStart = position.GetPositionAtOffset(index, LogicalDirection.Forward);
      TextPointer selectionEnd = selectionStart.GetPositionAtOffset(keywordLength, LogicalDirection.Forward);
      TextRange selection = new TextRange(selectionStart, selectionEnd);
      return selection;
    }
  }
}

