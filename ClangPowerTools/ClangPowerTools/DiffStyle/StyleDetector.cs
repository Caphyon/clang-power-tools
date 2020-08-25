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

    private List<string> filesContent;
    private Dictionary<EditorStyles, List<IFormatOption>> detectedStyles;
    private Dictionary<EditorStyles, int> stylesLevenshtein;
    private readonly Dictionary<EditorStyles, List<IFormatOption>> defaultStyles;

    private readonly List<string> filePaths = new List<string>()
    { "C:\\Users\\horat\\OneDrive\\Desktop\\A.cpp",
     // "C:\\Users\\horat\\OneDrive\\Desktop\\WW.cpp",
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
      filesContent = new List<string>();
      defaultStyles = CreateStyles();
    }

    #endregion

    #region Public Methods 

    public (EditorStyles matchedStyle, List<IFormatOption> matchedOptions) DetectStyleOptions(string input)
    {
      filesContent.Add(input);
      DetectFileStyle();
      DetectFileOptions();
      // TODO remove
      DetectStyleOptions(filePaths);

      return (FormatStyle, FormatOptions);
    }

    public (EditorStyles matchedStyle, List<IFormatOption> matchedOptions) DetectStyleOptions(List<string> filePaths)
    {
      filesContent = FileSystem.ReadContentFromMultipleFiles(filePaths);
      DetectFileStyle();
      DetectFileOptions();
      SetStyleByLevenshtein();

      return (FormatStyle, FormatOptions);
    }

    #endregion

    #region Private Methods

    private void DetectFileStyle()
    {
      // TODO could find column limit here
      var detectedPredefinedStyles = new Dictionary<EditorStyles, int>();
      foreach (var content in filesContent)
      {
        FindMatchingStyleForContent(content);
        if (detectedPredefinedStyles.ContainsKey(FormatStyle))
        {
          detectedPredefinedStyles[FormatStyle]++;
        }
        else
        {
          detectedPredefinedStyles.Add(FormatStyle, 1);
        }
      }
      detectedStyles = GetMatchingStyles(detectedPredefinedStyles);
    }

    private void DetectFileOptions()
    {
      stylesLevenshtein = new Dictionary<EditorStyles, int>();
      foreach (var content in filesContent)
      {
        foreach (var style in detectedStyles)
        {
          FormatStyle = style.Key;
          FormatOptions = style.Value;

          // TODO use threads
          foreach (var option in FormatOptions)
          {
            if (StopDetection) return;
            SetFormatOption(option, content);
          }

          if (detectedStyles.Count > 1)
          {
            AddLevenshteinForDetectedStyles(content);
          }
        }
      }
    }

    private Dictionary<EditorStyles, List<IFormatOption>> GetMatchingStyles(Dictionary<EditorStyles, int> detectedStyles)
    {
      var matchingStyles = new Dictionary<EditorStyles, List<IFormatOption>>();
      if (detectedStyles.Count == 1)
      {
        var style = detectedStyles.First().Key;
        var styleOptions = new List<IFormatOption>(defaultStyles[style]);
        matchingStyles.Add(style, styleOptions);

        return matchingStyles;
      }

      var sorted = detectedStyles.OrderBy(e => e.Value);
      var timesStyleFound = sorted.Last().Value;

      foreach (var item in sorted)
      {
        if (item.Value == timesStyleFound)
        {
          var style = item.Key;
          var styleOptions = new List<IFormatOption>(defaultStyles[style]);
          matchingStyles.Add(style, styleOptions);
        }
      }
      return matchingStyles;
    }

    private void FindMatchingStyleForContent(string content)
    {
      var levenshteinDiffs = new List<int>();
      var styleFormatter = new StyleFormatter();

      foreach (var style in defaultStyles)
      {
        if (StopDetection) return;
        var diffMatchPatchWrapper = new DiffMatchPatchWrapper();
        var formattedText = styleFormatter.FormatText(content, style.Value, style.Key);
        diffMatchPatchWrapper.Diff(content, formattedText);

        levenshteinDiffs.Add(diffMatchPatchWrapper.DiffLevenshtein());
      }

      var minLevenshtein = GetIndexOfSmallestLevenshtein(levenshteinDiffs);
      var matchedStyle = defaultStyles.ElementAt(minLevenshtein);

      FormatStyle = matchedStyle.Key;
      FormatOptions = matchedStyle.Value;
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
        inputValuesLevenshtein.Add(modelToggle.Value, GetLevenshteinAfterFormat(input));

        modelToggle.Value = ToggleValues.True;
        inputValuesLevenshtein.Add(modelToggle.Value, GetLevenshteinAfterFormat(input));

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
      inputValuesLevenshtein.Add(modelToggle.BooleanCombobox, GetLevenshteinAfterFormat(input));

      modelToggle.BooleanCombobox = ToggleValues.True;
      inputValuesLevenshtein.Add(modelToggle.BooleanCombobox, GetLevenshteinAfterFormat(input));

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
        inputValuesLevenshtein.Add(item, GetLevenshteinAfterFormat(input));
      }

      var inputValue = inputValuesLevenshtein.OrderBy(e => e.Value).First();

      inputModel.Input = inputValue.Value == inputValuesLevenshtein[previousInput] ?
                         previousInput : inputValue.Key;
    }

    private void AddLevenshteinForDetectedStyles(string content)
    {
      if (stylesLevenshtein.ContainsKey(FormatStyle))
      {
        stylesLevenshtein[FormatStyle] += GetLevenshteinAfterFormat(content);
      }
      else
      {
        stylesLevenshtein.Add(FormatStyle, GetLevenshteinAfterFormat(content));
      }
    }

    private void SetStyleByLevenshtein()
    {
      if (StopDetection) return;
      var sorted = stylesLevenshtein.OrderByDescending(e => e.Value);
      FormatStyle = sorted.Last().Key;
      FormatOptions = detectedStyles[FormatStyle];
    }

    private int GetLevenshteinAfterFormat(string input)
    {
      var styleFormatter = new StyleFormatter();
      var formattedText = styleFormatter.FormatText(input, FormatOptions, FormatStyle);

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
      return new Dictionary<EditorStyles, List<IFormatOption>>
      {
        { EditorStyles.LLVM, new FormatOptionsData().FormatOptions },
        { EditorStyles.Google, new FormatOptionsGoogleData().FormatOptions },
        { EditorStyles.Chromium, new FormatOptionsChromiumData().FormatOptions },
        { EditorStyles.Microsoft, new FormatOptionsMicrosoftData().FormatOptions },
        { EditorStyles.Mozilla, new FormatOptionsMozillaData().FormatOptions },
        { EditorStyles.WebKit, new FormatOptionsWebKitData().FormatOptions }
      };
    }

    #endregion

  }
}
