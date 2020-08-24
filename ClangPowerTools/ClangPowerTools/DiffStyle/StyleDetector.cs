using ClangPowerTools.Helpers;
using ClangPowerTools.MVVM.Interfaces;
using ClangPowerTools.MVVM.Models;
using System.Collections.Generic;
using System.Linq;

namespace ClangPowerTools.DiffStyle
{
  public class StyleDetector
  {
    #region Members

    private readonly Dictionary<EditorStyles, List<IFormatOption>> styles;
    private readonly Dictionary<EditorStyles, int> detectedPredefinedStyles;
    private readonly StyleFormatter formatter;

    private List<string> filePaths = new List<string>()
    { "C:\\Users\\horat\\OneDrive\\Desktop\\A.cpp",
      "C:\\Users\\horat\\OneDrive\\Desktop\\WW.cpp",
      "C:\\Users\\horat\\OneDrive\\Desktop\\X.cpp",
      "C:\\Users\\horat\\OneDrive\\Desktop\\Z.cpp"
    };


    #endregion

    #region Properties

    public EditorStyles FormatStyle { get; set; }
    public List<IFormatOption> FormatOptions { get; set; }
    public static bool StopDetection { get; set; } = false;

    #endregion

    #region Constructor

    public StyleDetector()
    {
      detectedPredefinedStyles = new Dictionary<EditorStyles, int>();
      formatter = new StyleFormatter();
      styles = CreateStyles();
    }

    #endregion

    #region Public Methods 

    public (EditorStyles matchedStyle, List<IFormatOption> matchedOptions) DetectStyleOptions(string input)
    {
      FindClosestStyle(input);
      FindClosestMatchingFlags(input);
      DetectStyleOptions(filePaths);

      return (FormatStyle, FormatOptions);
    }

    public (EditorStyles matchedStyle, List<IFormatOption> matchedOptions) DetectStyleOptions(List<string> filePaths)
    {
      //this.filePaths = filePaths;
      MultipleFileStyleDetection();

      return (FormatStyle, FormatOptions);
    }

    #endregion

    #region Private Methods

    private void FindClosestMatchingFlags(string input)
    {
      foreach (var option in FormatOptions)
      {
        if (StopDetection) return;
        SetFormatOption(option, input);
      }
    }

    private void FindClosestStyle(string input)
    {
      var levenshteinDiffs = new List<int>();

      foreach (var style in styles)
      {
        if (StopDetection) return;
        var diffMatchPatchWrapper = new DiffMatchPatchWrapper();
        var formattedText = formatter.FormatText(input, style.Value, style.Key);
        diffMatchPatchWrapper.Diff(input, formattedText);

        levenshteinDiffs.Add(diffMatchPatchWrapper.DiffLevenshtein());
      }

      var minLevenshtein = GetIndexOfSmallestLevenshtein(levenshteinDiffs);
      var matchedStyle = styles.ElementAt(minLevenshtein);

      FormatStyle = matchedStyle.Key;
      FormatOptions = matchedStyle.Value;
    }

    private void MultipleFileStyleDetection()
    {
      foreach (var path in filePaths)
      {
        var input = FileSystem.ReadContentToFile(path);
        if (string.IsNullOrWhiteSpace(input)) continue;

        FindClosestStyle(input);
        if (detectedPredefinedStyles.ContainsKey(FormatStyle))
        {
          detectedPredefinedStyles[FormatStyle]++;
        }
        else
        {
          detectedPredefinedStyles.Add(FormatStyle, 1);
        }
      }
      var maxStyle = detectedPredefinedStyles.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;

      FormatStyle = maxStyle;
      FormatOptions = styles[maxStyle];
    }

    private void MultipleFileFlagsDetection()
    {
      // TODO 
    }

    /// <summary>
    /// Set all possible values to the MultipleToggleModel and e use Levenshtein Diff to find the best one
    /// </summary>
    /// <param name="multipleToggleModel"></param>
    /// <param name="input"></param>
    private void SetOptionMultipleToggle(FormatOptionMultipleToggleModel multipleToggleModel, string input)
    {
      var toggleValues = multipleToggleModel.ToggleFlags;
      var inputValuesLevenshtein = new Dictionary<ToggleValues, int>();

      foreach (var modelToggle in toggleValues)
      {
        var previousInput = modelToggle.Value;

        modelToggle.Value = ToggleValues.False;
        inputValuesLevenshtein.Add(modelToggle.Value, GetLevenshteinAfterOptionChange(input));

        modelToggle.Value = ToggleValues.True;
        inputValuesLevenshtein.Add(modelToggle.Value, GetLevenshteinAfterOptionChange(input));

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
    /// <param name="input"></param>
    private void SetOptionToggle(FormatOptionToggleModel modelToggle, string input)
    {
      var previousInput = modelToggle.BooleanCombobox;
      var inputValuesLevenshtein = new Dictionary<ToggleValues, int>();

      modelToggle.BooleanCombobox = ToggleValues.False;
      inputValuesLevenshtein.Add(modelToggle.BooleanCombobox, GetLevenshteinAfterOptionChange(input));

      modelToggle.BooleanCombobox = ToggleValues.True;
      inputValuesLevenshtein.Add(modelToggle.BooleanCombobox, GetLevenshteinAfterOptionChange(input));

      var inputValue = inputValuesLevenshtein.OrderBy(e => e.Value).First();

      modelToggle.BooleanCombobox = inputValue.Value == inputValuesLevenshtein[previousInput] ?
                                    previousInput : inputValue.Key;
    }

    /// <summary>
    /// Set all possible values to the OptionInput and use Levenshtein Diff to find best one
    /// </summary>
    /// <param name="inputModel"></param>
    /// <param name="input"></param>
    private void SetOptionInput(FormatOptionInputModel inputModel, string input)
    {
      if (FormatOptionsInputValues.inputValues.ContainsKey(inputModel.Name) == false) return;

      string[] inputValues = FormatOptionsInputValues.inputValues[inputModel.Name];
      var previousInput = inputModel.Input;

      var inputValuesLevenshtein = new Dictionary<string, int>();

      foreach (var item in inputValues)
      {
        if (StopDetection) return;
        inputModel.Input = item;
        inputValuesLevenshtein.Add(item, GetLevenshteinAfterOptionChange(input));
      }

      var inputValue = inputValuesLevenshtein.OrderBy(e => e.Value).First();

      inputModel.Input = inputValue.Value == inputValuesLevenshtein[previousInput] ?
                         previousInput : inputValue.Key;
    }

    private int GetLevenshteinAfterOptionChange(string input)
    {
      var formattedText = formatter.FormatText(input, FormatOptions, FormatStyle);

      var diffMatchPatchWrapper = new DiffMatchPatchWrapper();
      diffMatchPatchWrapper.Diff(input, formattedText);

      return diffMatchPatchWrapper.DiffLevenshtein();
    }

    private int GetIndexOfSmallestLevenshtein(List<int> levenshteinDiffs)
    {
      var minLevenshtein = levenshteinDiffs.Min();
      return levenshteinDiffs.IndexOf(minLevenshtein);
    }

    private void SetFormatOption(IFormatOption formatOption, string input)
    {
      switch (formatOption)
      {
        case FormatOptionToggleModel toggleModel:
          SetOptionToggle(toggleModel, input);
          break;
        case FormatOptionInputModel inputModel:
          SetOptionInput(inputModel, input);
          break;
        case FormatOptionMultipleToggleModel multipleToggleModel:
          SetOptionMultipleToggle(multipleToggleModel, input);
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
