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
    private readonly ConcurrentDictionary<EditorStyles, (List<IFormatOption>, int)> styleOptions;
    private readonly ConcurrentDictionary<EditorStyles, int> detectedPredefinedStyles;
    private readonly Dictionary<EditorStyles, List<IFormatOption>> defaultStyles;
    private readonly object defaultLock;

    private readonly List<string> filePaths = new List<string>()
    { "C:\\Users\\horat\\OneDrive\\Desktop\\A.cpp",
     // "C:\\Users\\horat\\OneDrive\\Desktop\\WW.cpp",
      "C:\\Users\\horat\\OneDrive\\Desktop\\X.cpp",
      "C:\\Users\\horat\\OneDrive\\Desktop\\Z.cpp"
    };


    #endregion

    #region Properties

    public static bool StopDetection { get; set; } = false;

    #endregion

    #region Constructor

    public StyleDetector()
    {
      styleOptions = new ConcurrentDictionary<EditorStyles, (List<IFormatOption>, int)>();
      detectedPredefinedStyles = new ConcurrentDictionary<EditorStyles, int>();
      filesContent = new List<string>();
      defaultLock = new object();
      defaultStyles = CreateStyles();
    }

    #endregion

    #region Public Methods 

    public async Task<(EditorStyles matchedStyle, List<IFormatOption> matchedOptions)> DetectStyleOptionsAsync(string input)
    {
      filesContent.Add(input);
      await Task.WhenAll(filesContent.Select(e => DetectFileStyleAsync(e)));
      var detectedStyles = GetMatchingStyles(detectedPredefinedStyles);
      await Task.WhenAll(detectedStyles.Select(e => DetectFileOptionsAsync(e)));

      //await DetectStyleOptionsAsync(filePaths);

      return GetStyleByLevenshtein(styleOptions);
    }

    public async Task<(EditorStyles matchedStyle, List<IFormatOption> matchedOptions)> DetectStyleOptionsAsync(List<string> filePaths)
    {
      filesContent = FileSystem.ReadContentFromMultipleFiles(filePaths);

      await Task.WhenAll(filesContent.Select(e => DetectFileStyleAsync(e)));
      var detectedStyles = GetMatchingStyles(detectedPredefinedStyles);
      await Task.WhenAll(detectedStyles.Select(e => DetectFileOptionsAsync(e)));

      return GetStyleByLevenshtein(styleOptions);
    }

    #endregion

    #region Private Methods

    private async Task DetectFileStyleAsync(string content)
    {
      await Task.Run(() =>
      {
        foreach (var style in defaultStyles)
        {
          var levenshtein = GetLevenshteinAfterFormat(content, style.Key, style.Value);
          lock (defaultLock)
          {
            if (detectedPredefinedStyles.ContainsKey(style.Key))
            {
              detectedPredefinedStyles[style.Key] += levenshtein;
            }
            else
            {
              detectedPredefinedStyles.TryAdd(style.Key, levenshtein);
            }
          }
        }
      });
    }

    private async Task DetectFileOptionsAsync(EditorStyles style)
    {
      await Task.Run(() =>
      {
        var formatOptions = new List<IFormatOption>(defaultStyles[style]);
        var levenshtein = 0;
        foreach (var content in filesContent)
        {
          foreach (var option in formatOptions)
          {
            SetFormatOption(option, content, style, formatOptions);
          }
          levenshtein += GetLevenshteinAfterFormat(content, style, formatOptions);
        }
        lock (defaultLock)
        {
          styleOptions.TryAdd(style, (formatOptions, levenshtein));
        }
      });
    }


    private HashSet<EditorStyles> GetMatchingStyles(ConcurrentDictionary<EditorStyles, int> detectedStyles)
    {
      var matchingStyles = new HashSet<EditorStyles>();
      var sorted = detectedStyles.OrderBy(e => e.Value);
      var timesStyleFound = sorted.First().Value;

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

    private (EditorStyles, List<IFormatOption>) GetStyleByLevenshtein(ConcurrentDictionary<EditorStyles, (List<IFormatOption>, int)> stylesLevenshtein)
    {
      var sorted = stylesLevenshtein.OrderBy(e => e.Value.Item2).First();
      return (sorted.Key, sorted.Value.Item1);
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
