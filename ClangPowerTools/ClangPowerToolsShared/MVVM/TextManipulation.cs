﻿using System;
using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows.Media;

namespace ClangPowerTools
{
  /// <summary>
  /// Class for text manipulation operations
  /// </summary>
  public class TextManipulation
  {
    public static void HighlightKeywords(TextPointer startPointer, TextPointer endPointer, HashSet<string> keywords, Brush foreground)
    {
        if (startPointer == null) throw new ArgumentNullException(nameof(startPointer));
        if (endPointer == null) throw new ArgumentNullException(nameof(endPointer));

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

          ChangeTextColor(keywords, foreground, text, position, textInRun);
          position = position.GetNextContextPosition(LogicalDirection.Forward);
        }
    }

    public static void ReplaceAllTextInFlowDocument(FlowDocument document, string text)
    {
      TextRange textRange = new TextRange(document.ContentStart, document.ContentEnd);
      textRange.Text = text;
    }

    private static void ChangeTextColor(HashSet<string> keywords, Brush foreground, TextRange text, TextPointer position, string textInRun)
    {
      foreach (var keyword in keywords)
      {
        int index = textInRun.IndexOf(keyword);
        if (index != -1 && CheckForSpaceAfterKeyword(textInRun, keyword, index))
        {
          TextRange selection = CreateSelection(position, keyword.Length, index);
          selection.ApplyPropertyValue(TextElement.ForegroundProperty, foreground);
        }
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

