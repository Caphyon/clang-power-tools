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
      editorInput = text;

      var (matchedStyle, matchedOptions) = GetClosestDefaultStyle(text);
      formatStyle = matchedStyle;
      formatOptions = matchedOptions;

      foreach (var option in formatOptions)
      {
        IncludeFormatOption(option);
      }

      ShowHtmlAfterDiff();

      //return (EditorStyles matchedStyle, List<IFormatOption> matchedOptions)
    }

    #endregion

    #region Private Methods

    private (EditorStyles matchedStyle, List<IFormatOption> matchedOptions) GetClosestDefaultStyle(string input)
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
      return (matchedStyle.Key, matchedStyle.Value);
    }

    private int GetSmallestLevenshtein(List<int> levenshteinDiffs)
    {
      var minLevenshtein = levenshteinDiffs.Min();
      return levenshteinDiffs.IndexOf(minLevenshtein);
    }

    private void IncludeFormatOption(IFormatOption formatOption)
    {
      switch (formatOption)
      {
        case FormatOptionToggleModel toggleModel:
          IncludeOptionToggle(toggleModel);
          break;
        case FormatOptionInputModel inputModel:
          IncludeOptionInput(inputModel);
          break;
        default:
          break;
      }
    }

    private void IncludeOptionToggle(FormatOptionToggleModel modelToggle)
    {
      var previousInput = modelToggle.BooleanCombobox;
      Dictionary<ToggleValues, int> inputValuesLevenshtein = new Dictionary<ToggleValues, int>();

      modelToggle.BooleanCombobox = ToggleValues.False;
      inputValuesLevenshtein.Add(ToggleValues.False, GetLevenshteinAfterOptionChange());

      modelToggle.BooleanCombobox = ToggleValues.True;
      inputValuesLevenshtein.Add(ToggleValues.True, GetLevenshteinAfterOptionChange());

      var inputValue = inputValuesLevenshtein.OrderBy(e => e.Value).First();
      if (inputValue.Value == inputValuesLevenshtein[previousInput])
      {
        modelToggle.BooleanCombobox = previousInput;
      }
      else
      {
        modelToggle.BooleanCombobox = inputValue.Key;
      }
    }

    private void IncludeOptionInput(FormatOptionInputModel inputModel)
    {
      if (FormatOptionsInputValues.inputValues.ContainsKey(inputModel.Name) == false) return;

      string[] inputValues = FormatOptionsInputValues.inputValues[inputModel.Name];
      var previousInput = inputModel.Input;

      Dictionary<string, int> inputValuesLevenshtein = new Dictionary<string, int>();

      foreach (var item in inputValues)
      {
        inputModel.Input = item;
        inputValuesLevenshtein.Add(item, GetLevenshteinAfterOptionChange());
      }

      var inputValue = inputValuesLevenshtein.OrderBy(e => e.Value).First();
      if (inputValue.Value == inputValuesLevenshtein[previousInput])
      {
        inputModel.Input = previousInput;
      }
      else
      {
        inputModel.Input = inputValue.Key;
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
