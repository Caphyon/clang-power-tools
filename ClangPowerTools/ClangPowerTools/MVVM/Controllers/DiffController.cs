using ClangPowerTools.MVVM.Interfaces;
using ClangPowerTools.MVVM.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClangPowerTools.MVVM.Controllers
{
  public class DiffController
  {
    #region Members

    private Formatter formatter;
    private readonly Dictionary<EditorStyles, List<IFormatOption>> styles;

    #endregion


    #region Constructor

    public DiffController()
    {
      formatter = new Formatter();
      styles = CreateStyles();
    }

    #endregion

    #region Methods

    public void CreateConfigUsingDiff(string editorInput)
    {
      var (matchedStyle, matchedOptions) = GetClosestMatchingStyle(editorInput);

      var formattedText = formatter.FormatText(editorInput, matchedOptions, matchedStyle);

      var diffMatchPatchWrapper = new DiffMatchPatchWrapper();
      diffMatchPatchWrapper.Diff(editorInput, formattedText);
      //diffMatchPatchWrapper.CleanupSemantic();


      var styleName = Enum.GetName(typeof(EditorStyles), matchedStyle);
      var html = diffMatchPatchWrapper.DiffAsHtml() + "<br><br>" + styleName;
      ShowHtmlDiff(html);
    }

    (EditorStyles matchedStyle, List<IFormatOption> matchedOptions) GetClosestMatchingStyle(string text)
    {
      var levenshteinDiffs = new List<int>();

      foreach (var style in styles)
      {
        var diffMatchPatchWrapper = new DiffMatchPatchWrapper();
        var formattedText = formatter.FormatText(text, style.Value, style.Key);
        diffMatchPatchWrapper.Diff(text, formattedText);
        //diffMatchPatchWrapper.CleanupEfficiency();
        //diffMatchPatchWrapper.CleanupSemantic();
        //var html = diffMatchPatchWrapper.DiffAsHtml() + "<br><br>";
        //ShowHtmlDiff(html);

        levenshteinDiffs.Add(diffMatchPatchWrapper.DiffLevenshtein());
      }

      var index = GetSmallestLevenshtein(levenshteinDiffs);
      var matchedStyle = styles.ElementAt(index);
      return (matchedStyle.Key, matchedStyle.Value);
    }

    private int GetSmallestLevenshtein(List<int> levenshteinDiffs)
    {
      var minLevenshtein = levenshteinDiffs.Min();
      return levenshteinDiffs.IndexOf(minLevenshtein);
    }

    private void ShowHtmlDiff(string html)
    {
      var diffWindow = new DiffWindow(html);
      diffWindow.Show();
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

    #endregion
  }
}
