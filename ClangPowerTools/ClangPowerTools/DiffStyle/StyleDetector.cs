using ClangPowerTools.Helpers;
using ClangPowerTools.MVVM.Interfaces;
using ClangPowerTools.MVVM.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClangPowerTools.DiffStyle
{
  public class StyleDetector
  {
    #region Members

    private List<string> filesContent;
    private readonly ConcurrentDictionary<EditorStyles, List<IFormatOption>> styleOptions;
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
      styleOptions = new ConcurrentDictionary<EditorStyles, List<IFormatOption>>();
      filesContent = new List<string>();
      defaultStyles = CreateStyles();
    }

    #endregion

    #region Public Methods 

    public async Task<(EditorStyles matchedStyle, List<IFormatOption> matchedOptions)> DetectStyleOptionsAsync(string input)
    {
      filesContent.Add(input);
      var detectedStyles = DetectFileStyle();
      //await DetectFileOptionsAsync(detectedStyles);

      await DetectStyleOptionsAsync(filePaths);

      return (FormatStyle, FormatOptions);
    }

    public async Task<(EditorStyles matchedStyle, List<IFormatOption> matchedOptions)> DetectStyleOptionsAsync(List<string> filePaths)
    {
      filesContent = FileSystem.ReadContentFromMultipleFiles(filePaths);
      var detectedStyles = DetectFileStyle();
      await DetectFileOptionsAsync(detectedStyles);

      return (FormatStyle, FormatOptions);
    }

    #endregion

    #region Private Methods

    private HashSet<EditorStyles> DetectFileStyle()
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
      return GetMatchingStyles(detectedPredefinedStyles);
    }

    private async Task DetectFileOptionsAsync(HashSet<EditorStyles> detectedStyles)
    {
      var stylesLevenshtein = new ConcurrentDictionary<EditorStyles, int>();

      await Task.WhenAll(filesContent.Select(e => TestAsync(detectedStyles, stylesLevenshtein, e)));
      if (detectedStyles.Count > 1)
      {
        SetStyleByLevenshtein(stylesLevenshtein);
      }
      else
      {
        FormatStyle = detectedStyles.First();
        FormatOptions = styleOptions[FormatStyle];
      }

    }


    private async Task TestAsync(HashSet<EditorStyles> detectedStyles, ConcurrentDictionary<EditorStyles, int> stylesLevenshtein, string content)
    {
      foreach (var style in detectedStyles)
      {
        await Task.Run(() =>
        {
          var formatOptions = new List<IFormatOption>(defaultStyles[style]);
          styleOptions.TryAdd(style, formatOptions);
          foreach (var option in formatOptions)
          {
            SetFormatOption(option, content, style, formatOptions);
          }

          if (detectedStyles.Count > 1)
          {
            if (stylesLevenshtein.ContainsKey(style))
            {
              stylesLevenshtein[style] += GetLevenshteinAfterFormat(content, style, formatOptions);
            }
            else
            {
              stylesLevenshtein.TryAdd(style, GetLevenshteinAfterFormat(content, style, formatOptions));
            }
          }
        });
      }
    }

    private HashSet<EditorStyles> GetMatchingStyles(Dictionary<EditorStyles, int> detectedStyles)
    {
      var matchingStyles = new HashSet<EditorStyles>();
      if (detectedStyles.Count == 1)
      {
        matchingStyles.Add(detectedStyles.First().Key);
        return matchingStyles;
      }

      var sorted = detectedStyles.OrderBy(e => e.Value);
      var timesStyleFound = sorted.Last().Value;

      foreach (var item in sorted)
      {
        if (item.Value == timesStyleFound)
        {
          var style = item.Key;
          matchingStyles.Add(style);
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

    private void SetStyleByLevenshtein(ConcurrentDictionary<EditorStyles, int> stylesLevenshtein)
    {
      var sorted = stylesLevenshtein.OrderByDescending(e => e.Value);
      FormatStyle = sorted.Last().Key;
      FormatOptions = styleOptions[FormatStyle];
    }


    /// <summary>
    /// Set all possible values to the MultipleToggleModel and e use Levenshtein Diff to find the best one
    /// </summary>
    /// <param name="multipleToggleModel"></param>
    /// <param name="input"></param>
    private void SetOptionMultipleToggle(FormatOptionMultipleToggleModel multipleToggleModel, string input, List<IFormatOption> formatOptions, EditorStyles formatStyle)
    {
      var toggleValues = multipleToggleModel.ToggleFlags;
      var inputValuesLevenshtein = new Dictionary<ToggleValues, int>();

      foreach (var modelToggle in toggleValues)
      {
        var previousInput = modelToggle.Value;

        modelToggle.Value = ToggleValues.False;
        inputValuesLevenshtein.Add(modelToggle.Value, GetLevenshteinAfterFormat(input, formatStyle, formatOptions));

        modelToggle.Value = ToggleValues.True;
        inputValuesLevenshtein.Add(modelToggle.Value, GetLevenshteinAfterFormat(input, formatStyle, formatOptions));

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
    private void SetOptionToggle(FormatOptionToggleModel modelToggle, string input, EditorStyles formatStyle, List<IFormatOption> formatOptions)
    {
      var previousInput = modelToggle.BooleanCombobox;
      var inputValuesLevenshtein = new Dictionary<ToggleValues, int>();

      modelToggle.BooleanCombobox = ToggleValues.False;
      inputValuesLevenshtein.Add(modelToggle.BooleanCombobox, GetLevenshteinAfterFormat(input, formatStyle, formatOptions));

      modelToggle.BooleanCombobox = ToggleValues.True;
      inputValuesLevenshtein.Add(modelToggle.BooleanCombobox, GetLevenshteinAfterFormat(input, formatStyle, formatOptions));

      var inputValue = inputValuesLevenshtein.OrderBy(e => e.Value).First();

      modelToggle.BooleanCombobox = inputValue.Value == inputValuesLevenshtein[previousInput] ?
                                    previousInput : inputValue.Key;
    }

    /// <summary>
    /// Set all possible values to the OptionInput and use Levenshtein Diff to find best one
    /// </summary>
    /// <param name="inputModel"></param>
    /// <param name="input"></param>
    private void SetOptionInput(FormatOptionInputModel inputModel, string input, EditorStyles formatStyle, List<IFormatOption> formatOptions)
    {
      if (FormatOptionsInputValues.inputValues.ContainsKey(inputModel.Name) == false) return;

      string[] inputValues = FormatOptionsInputValues.inputValues[inputModel.Name];
      var previousInput = inputModel.Input;

      var inputValuesLevenshtein = new Dictionary<string, int>();

      foreach (var item in inputValues)
      {
        if (StopDetection) return;
        inputModel.Input = item;
        inputValuesLevenshtein.Add(item, GetLevenshteinAfterFormat(input, formatStyle, formatOptions));
      }

      var inputValue = inputValuesLevenshtein.OrderBy(e => e.Value).First();

      inputModel.Input = inputValue.Value == inputValuesLevenshtein[previousInput] ?
                         previousInput : inputValue.Key;
    }

    private int GetLevenshteinAfterFormat(string input, EditorStyles formatStyle, List<IFormatOption> formatOptions)
    {
      var styleFormatter = new StyleFormatter();
      var formattedText = styleFormatter.FormatText(input, formatOptions, formatStyle);

      var diffMatchPatchWrapper = new DiffMatchPatchWrapper();
      diffMatchPatchWrapper.Diff(input, formattedText);

      return diffMatchPatchWrapper.DiffLevenshtein();
    }

    private int GetIndexOfSmallestLevenshtein(List<int> levenshteinDiffs)
    {
      var minLevenshtein = levenshteinDiffs.Min();
      return levenshteinDiffs.IndexOf(minLevenshtein);
    }
    private void SetFormatOption(IFormatOption formatOption, string input, EditorStyles formatStyle, List<IFormatOption> formatOptions)
    {
      switch (formatOption)
      {
        case FormatOptionToggleModel toggleModel:
          SetOptionToggle(toggleModel, input, formatStyle, formatOptions);
          break;
        case FormatOptionInputModel inputModel:
          SetOptionInput(inputModel, input, formatStyle, formatOptions);
          break;
        case FormatOptionMultipleToggleModel multipleToggleModel:
          SetOptionMultipleToggle(multipleToggleModel, input, formatOptions, formatStyle);
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
