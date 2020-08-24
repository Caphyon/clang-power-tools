using ClangPowerTools.MVVM.Interfaces;
using ClangPowerTools.MVVM.Models;
using System.Collections.Generic;
using System.Linq;

namespace ClangPowerTools.DiffStyle
{
  public class StyleDetector
  {
    #region Members

    private EditorStyles formatStyle;
    private List<IFormatOption> formatOptions;
    private string input;
    private bool stopDetection = false;

    private readonly StyleFormatter formatter;
    private readonly Dictionary<EditorStyles, List<IFormatOption>> styles;

    #endregion

    #region Constructor

    public StyleDetector()
    {
      formatter = new StyleFormatter();
      styles = CreateStyles();
    }

    #endregion

    #region Public Methods 


    public (EditorStyles matchedStyle, List<IFormatOption> matchedOptions) DetectStyleOptions(string input)
    {
      this.input = input;
      FindClosestDefaultStyle();
      FindClosestMatchingFlags();

      return (formatStyle, formatOptions);
    }

    #endregion

    #region Private Methods

    private void FindClosestMatchingFlags()
    {
      foreach (var option in formatOptions)
      {
        if (stopDetection) return;
        SetFormatOption(option);
      }
    }

    private void FindClosestDefaultStyle()
    {
      var levenshteinDiffs = new List<int>();

      foreach (var style in styles)
      {
        var diffMatchPatchWrapper = new DiffMatchPatchWrapper();
        var formattedText = formatter.FormatText(input, style.Value, style.Key);
        diffMatchPatchWrapper.Diff(input, formattedText);

        levenshteinDiffs.Add(diffMatchPatchWrapper.DiffLevenshtein());
      }

      var minLevenshtein = GetIndexOfSmallestLevenshtein(levenshteinDiffs);
      var matchedStyle = styles.ElementAt(minLevenshtein);

      formatStyle = matchedStyle.Key;
      formatOptions = matchedStyle.Value;
    }

    /// <summary>
    /// Set all possible values to the MultipleToggleModel and e use Levenshtein Diff to find the best one
    /// </summary>
    /// <param name="multipleToggleModel"></param>
    private void SetOptionMultipleToggle(FormatOptionMultipleToggleModel multipleToggleModel)
    {
      var toggleValues = multipleToggleModel.ToggleFlags;
      var inputValuesLevenshtein = new Dictionary<ToggleValues, int>();

      foreach (var modelToggle in toggleValues)
      {
        var previousInput = modelToggle.Value;

        modelToggle.Value = ToggleValues.False;
        inputValuesLevenshtein.Add(modelToggle.Value, GetLevenshteinAfterOptionChange());

        modelToggle.Value = ToggleValues.True;
        inputValuesLevenshtein.Add(modelToggle.Value, GetLevenshteinAfterOptionChange());

        var inputValue = inputValuesLevenshtein.OrderBy(e => e.Value).First();
        modelToggle.Value = inputValue.Value == inputValuesLevenshtein[previousInput] ?
                                      previousInput : inputValue.Key;

        inputValuesLevenshtein.Clear();
      }
    }

    /// <summary>
    /// Set all possible values to the OptionToggle and e use Levenshtein Diff to find the best one
    /// </summary>
    /// <param name="modelToggle"></param>
    private void SetOptionToggle(FormatOptionToggleModel modelToggle)
    {
      var previousInput = modelToggle.BooleanCombobox;
      var inputValuesLevenshtein = new Dictionary<ToggleValues, int>();

      modelToggle.BooleanCombobox = ToggleValues.False;
      inputValuesLevenshtein.Add(modelToggle.BooleanCombobox, GetLevenshteinAfterOptionChange());

      modelToggle.BooleanCombobox = ToggleValues.True;
      inputValuesLevenshtein.Add(modelToggle.BooleanCombobox, GetLevenshteinAfterOptionChange());

      var inputValue = inputValuesLevenshtein.OrderBy(e => e.Value).First();

      modelToggle.BooleanCombobox = inputValue.Value == inputValuesLevenshtein[previousInput] ?
                                    previousInput : inputValue.Key;
    }

    /// <summary>
    /// Set all possible values to the OptionInput and use Levenshtein Diff to find best one
    /// </summary>
    /// <param name="inputModel"></param>
    private void SetOptionInput(FormatOptionInputModel inputModel)
    {
      if (FormatOptionsInputValues.inputValues.ContainsKey(inputModel.Name) == false) return;

      string[] inputValues = FormatOptionsInputValues.inputValues[inputModel.Name];
      var previousInput = inputModel.Input;

      var inputValuesLevenshtein = new Dictionary<string, int>();

      foreach (var item in inputValues)
      {
        if (stopDetection) return;
        inputModel.Input = item;
        inputValuesLevenshtein.Add(item, GetLevenshteinAfterOptionChange());
      }

      var inputValue = inputValuesLevenshtein.OrderBy(e => e.Value).First();

      inputModel.Input = inputValue.Value == inputValuesLevenshtein[previousInput] ?
                         previousInput : inputValue.Key;
    }

    private int GetLevenshteinAfterOptionChange()
    {
      var formattedText = formatter.FormatText(input, formatOptions, formatStyle);

      var diffMatchPatchWrapper = new DiffMatchPatchWrapper();
      diffMatchPatchWrapper.Diff(input, formattedText);

      return diffMatchPatchWrapper.DiffLevenshtein();
    }

    private int GetIndexOfSmallestLevenshtein(List<int> levenshteinDiffs)
    {
      var minLevenshtein = levenshteinDiffs.Min();
      return levenshteinDiffs.IndexOf(minLevenshtein);
    }

    private void SetFormatOption(IFormatOption formatOption)
    {
      switch (formatOption)
      {
        case FormatOptionToggleModel toggleModel:
          SetOptionToggle(toggleModel);
          break;
        case FormatOptionInputModel inputModel:
          SetOptionInput(inputModel);
          break;
        case FormatOptionMultipleToggleModel multipleToggleModel:
          SetOptionMultipleToggle(multipleToggleModel);
          break;
        default:
          break;
      }
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
