using ClangPowerTools.MVVM.Interfaces;
using ClangPowerTools.MVVM.Models;
using ClangPowerTools.MVVM.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClangPowerTools.MVVM.Controllers
{
  public class DiffController
  {
    #region Members

    private readonly Formatter formatter;
    private readonly Dictionary<EditorStyles, List<IFormatOption>> styles;

    private EditorStyles formatStyle;
    private List<IFormatOption> formatOptions;
    private string editorInput;
    private int levenshtein;

    #endregion


    #region Constructor

    public DiffController()
    {
      formatter = new Formatter();
      styles = CreateStyles();
    }

    #endregion

    #region Public Methods

    public void FindStyleFromDiff(string text)
    {
      var (matchedStyle, matchedOptions, minLevenshtein) = GetClosestDefaultStyle(text);

      formatStyle = matchedStyle;
      formatOptions = matchedOptions;
      editorInput = text;
      levenshtein = minLevenshtein;

      foreach (var item in formatOptions)
      {
        CheckFormatOption(item);
      }

      ShowHtmlAfterDiff();

      //return (EditorStyles matchedStyle, List<IFormatOption> matchedOptions)
    }

    #endregion

    #region Private Methods

    private (EditorStyles matchedStyle, List<IFormatOption> matchedOptions, int levenshteinDiffs) GetClosestDefaultStyle(string input)
    {
      var levenshteinDiffs = new List<int>();

      foreach (var style in styles)
      {
        var diffMatchPatchWrapper = new DiffMatchPatchWrapper();
        var formattedText = formatter.FormatText(input, style.Value, style.Key);
        diffMatchPatchWrapper.Diff(input, formattedText);

        levenshteinDiffs.Add(diffMatchPatchWrapper.DiffLevenshtein());
      }

      var minLevenshtein = GetSmallestLevenshtein(levenshteinDiffs);
      var matchedStyle = styles.ElementAt(minLevenshtein);
      return (matchedStyle.Key, matchedStyle.Value, minLevenshtein);
    }

    private int GetSmallestLevenshtein(List<int> levenshteinDiffs)
    {
      var minLevenshtein = levenshteinDiffs.Min();
      return levenshteinDiffs.IndexOf(minLevenshtein);
    }

    private int CheckFormatOption(IFormatOption formatOption)
    {
      switch (formatOption)
      {
        case FormatOptionToggleModel modelToggle:
          if (modelToggle.BooleanCombobox == ToggleValues.True)
          {
            CheckOptionToggleLevenshtein(modelToggle, ToggleValues.True, ToggleValues.False);
          }
          else if (modelToggle.BooleanCombobox == ToggleValues.False)
          {
            CheckOptionToggleLevenshtein(modelToggle, ToggleValues.False, ToggleValues.True);
          }
          break;

        case FormatOptionInputModel modelInput:
          break;
        //case FormatOptionMultipleInputModel modelMultipleInput:
        //  break;
        default:
          break;
      }

      return 0;
    }

    private void CheckOptionToggleLevenshtein(FormatOptionToggleModel modelToggle, ToggleValues current, ToggleValues modified)
    {
      modelToggle.BooleanCombobox = modified;
      int levenshteinAfterChange = GetLevenshteinAfterOptionChange();
      if (levenshteinAfterChange < levenshtein)
      {
        levenshtein = levenshteinAfterChange;
      }
      else
      {
        modelToggle.BooleanCombobox = current;
      }
    }

    private int GetLevenshteinAfterOptionChange()
    {
      var formattedText = formatter.FormatText(editorInput, formatOptions, formatStyle);

      var diffMatchPatchWrapper = new DiffMatchPatchWrapper();
      diffMatchPatchWrapper.Diff(editorInput, formattedText);

      return diffMatchPatchWrapper.DiffLevenshtein();
    }


    private Dictionary<EditorStyles, List<IFormatOption>> CreateStyles()
    {
      var groupedStyles = new Dictionary<EditorStyles, List<IFormatOption>>
      {
        { EditorStyles.LLVM, new FormatOptionsData().FormatOptions },
        { EditorStyles.Google, new FormatOptionsGoogleData().FormatOptions },
        { EditorStyles.Chromium, new FormatOptionsChromiumData().FormatOptions },
        { EditorStyles.Microsoft, new FormatOptionsMicrosoftData().FormatOptions },
        { EditorStyles.Mozilla, new FormatOptionsMozillaData().FormatOptions },
        { EditorStyles.WebKit, new FormatOptionsWebKitData().FormatOptions }
      };

      return groupedStyles;
    }

    private void ShowHtmlAfterDiff()
    {
      var formattedText = formatter.FormatText(editorInput, formatOptions, formatStyle);
      var diffMatchPatchWrapper = new DiffMatchPatchWrapper();
      diffMatchPatchWrapper.Diff(editorInput, formattedText);
      diffMatchPatchWrapper.CleanupSemantic();


      var styleName = Enum.GetName(typeof(EditorStyles), formatStyle);
      var html = diffMatchPatchWrapper.DiffAsHtml() + "<br><br>" + styleName;

      var diffWindow = new DiffWindow(html);
      diffWindow.Show();
    }

    #endregion
  }
}
