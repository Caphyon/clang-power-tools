using ClangPowerTools.MVVM.Interfaces;
using ClangPowerTools.MVVM.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ClangPowerTools.DiffStyle
{
  public class StyleDetector
  {
    #region Members

    private EditorStyles detectedStyle;
    private List<string> filesContent;
    private Dictionary<string, (IFormatOption, bool)> styleOptions;
    private readonly Dictionary<EditorStyles, int> detectedPredefinedStyles;
    private readonly Dictionary<EditorStyles, List<IFormatOption>> defaultStyles;
    private readonly ConcurrentBag<string> customInput;
    private readonly ConcurrentBag<int> columnLimits;
    private readonly ConcurrentBag<int> tabWidths;
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
      styleOptions = new Dictionary<string, (IFormatOption, bool)>();
      detectedPredefinedStyles = new Dictionary<EditorStyles, int>();
      filesContent = new List<string>();
      columnLimits = new ConcurrentBag<int>();
      tabWidths = new ConcurrentBag<int>();
      defaultLock = new object();
      customInput = new ConcurrentBag<string>() { "TabWidth", "ColumnLimit" };
      defaultStyles = CreateStyles();
    }

    #endregion

    #region Public Methods 

    public async Task<(EditorStyles matchedStyle, List<IFormatOption> matchedOptions)> DetectStyleOptionsAsync(string input)
    {
      //filesContent.Add(input);
      //var stopwatch = new Stopwatch();
      //stopwatch.Start();
      //await DetectAsync();
      return await DetectStyleOptionsAsync(filePaths);
      //var options = new List<IFormatOption>();
      ////foreach (var item in styleOptions)
      ////{
      ////  options.Add(item.Value.Item1);
      ////}
      ////stopwatch.Stop();
      //return (detectedStyle, options);
    }

    public async Task<(EditorStyles matchedStyle, List<IFormatOption> matchedOptions)> DetectStyleOptionsAsync(List<string> filePaths)
    {
      //filesContent = FileSystem.ReadContentFromMultipleFiles(filePaths);
      var cpps = Directory.GetFiles("C:\\Users\\horat\\OneDrive\\Documente\\ai_advinst\\custact", "*.cpp", SearchOption.AllDirectories);
      var hs = Directory.GetFiles("C:\\Users\\horat\\OneDrive\\Documente\\ai_advinst\\custact", "*.h", SearchOption.AllDirectories);
      filesContent.AddRange(cpps);

      var watch = new Stopwatch();
      watch.Start();
      await DetectAsync();


      var options = new List<IFormatOption>();
      foreach (var item in styleOptions)
      {
        options.Add(item.Value.Item1);
      }
      watch.Stop();
      return (detectedStyle, options);
    }

    #endregion

    #region Private Methods

    private async Task DetectAsync()
    {
      await Task.WhenAll(filesContent.Select(e => CalculateColumTabAsync(e)));
      await Task.WhenAll(filesContent.Select(e => DetectFileStyleAsync(e)));
      detectedStyle = GetStyleByLevenshtein(detectedPredefinedStyles);
      styleOptions = CreateDetectedStyleOptions(detectedStyle);

      await Task.WhenAll(filesContent.Select(e => DetectFileOptionsAsync(e, detectedStyle)));
    }

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
              detectedPredefinedStyles.Add(style.Key, levenshtein);
            }
          }
        }
      });
    }

    private async Task DetectFileOptionsAsync(string content, EditorStyles style)
    {
      await Task.Run(() =>
      {
        var formatOptions = new List<IFormatOption>(defaultStyles[style]);
        foreach (var option in formatOptions)
        {
          SetFormatOption(option, content, style, formatOptions);
        }
      });
    }

    private EditorStyles GetStyleByLevenshtein(Dictionary<EditorStyles, int> stylesLevenshtein)
    {
      var sorted = stylesLevenshtein.OrderBy(e => e.Value).First();
      return sorted.Key;
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
      bool modelChanged = false;

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

        if (inputValue.Value != inputValuesLevenshtein[previousInput]) modelChanged = true;
        inputValuesLevenshtein.Clear();
      }

      lock (defaultLock)
      {
        if (modelChanged)
        {
          if (styleOptions[multipleToggleModel.Name].Item2 == false)
          {
            styleOptions[multipleToggleModel.Name] = (multipleToggleModel, true);
          }
        }
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

      lock (defaultLock)
      {
        if (inputValue.Value == inputValuesLevenshtein[previousInput])
        {
          modelToggle.BooleanCombobox = previousInput;
        }
        else
        {
          modelToggle.BooleanCombobox = inputValue.Key;
          if (styleOptions[modelToggle.Name].Item2 == false)
          {
            styleOptions[modelToggle.Name] = (modelToggle, true);
          }
        }
      }
    }

    /// <summary>
    /// Set all possible values to the OptionInput and use Levenshtein Diff to find best one
    /// </summary>
    /// <param name="inputModel"></param>
    /// <param name="input"></param>
    private void SetOptionInput(FormatOptionInputModel inputModel, string input, EditorStyles formatStyle, List<IFormatOption> formatOptions)
    {
      if (FormatOptionsInputValues.inputValues.ContainsKey(inputModel.Name) == false) return;

      if (customInput.Contains(inputModel.Name))
      {
        if (int.TryParse(inputModel.Input, out int result))
        {
          inputModel.Input = SetColumnTab(result, inputModel.Name);
        }
      }
      else
      {
        var inputValuesLevenshtein = new Dictionary<string, int>();
        string[] inputValues = FormatOptionsInputValues.inputValues[inputModel.Name];
        var previousInput = inputModel.Input;
        foreach (var item in inputValues)
        {
          if (StopDetection) return;
          inputModel.Input = item;
          inputValuesLevenshtein.Add(item, GetLevenshteinAfterFormat(input, formatStyle, formatOptions));
        }

        var inputValue = inputValuesLevenshtein.OrderBy(e => e.Value).First();

        lock (defaultLock)
        {
          if (inputValue.Value == inputValuesLevenshtein[previousInput])
          {
            inputModel.Input = previousInput;
          }
          else
          {
            inputModel.Input = inputValue.Key;
            if (styleOptions[inputModel.Name].Item2 == false)
            {
              styleOptions[inputModel.Name] = (inputModel, true);
            }
          }
        }
      }
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

    private Dictionary<string, (IFormatOption, bool)> CreateDetectedStyleOptions(EditorStyles style)
    {
      var defaultOptions = new List<IFormatOption>(defaultStyles[style]);
      var options = new Dictionary<string, (IFormatOption, bool)>();
      foreach (var item in defaultOptions)
      {
        options.Add(item.Name, (item, false));
      }
      return options;
    }

    private async Task CalculateColumTabAsync(string content)
    {
      await Task.Run(() =>
      {
        var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        var lineLengths = new List<int>();
        var tabs = new List<int>();
        foreach (var line in lines)
        {
          lineLengths.Add(line.Length);
          var tabCount = line.TakeWhile(e => e == '\t').Count();
          tabs.Add(tabCount);
        }

        columnLimits.Add(lineLengths.Max());
        tabWidths.Add(tabs.Max());
      });
    }

    private string SetColumnTab(int optionInput, string optionName)
    {
      switch (optionName)
      {
        case "TabWidth":
          var maxTabWidth = tabWidths.Max();
          if (maxTabWidth > optionInput)
          {
            return maxTabWidth.RoundUp().ToString();
          }
          break;
        case "ColumnLimit":
          var maxColumnLimit = columnLimits.Max();
          if (maxColumnLimit > optionInput)
          {
            return maxColumnLimit.RoundUp().ToString();
          }
          break;
        default:
          break;
      }
      return optionInput.ToString();
    }


    #endregion

  }
}
