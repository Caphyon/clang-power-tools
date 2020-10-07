using ClangPowerTools.MVVM.Interfaces;
using ClangPowerTools.MVVM.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClangPowerTools.DiffStyle
{
  public class StyleDetector
  {
    #region Members

    private EditorStyles detectedStyle;
    private List<string> filesContent;
    private readonly Dictionary<EditorStyles, int> detectedPredefinedStyles;
    private readonly ConcurrentBag<string> customInput;
    private readonly ConcurrentBag<int> columnLimits;
    private readonly ConcurrentBag<int> tabWidths;
    private readonly ConcurrentBag<List<IFormatOption>> allFoundOptions;
    private readonly object defaultLock;

    #endregion

    #region Constructor

    public StyleDetector()
    {
      detectedPredefinedStyles = new Dictionary<EditorStyles, int>();
      filesContent = new List<string>();
      columnLimits = new ConcurrentBag<int>();
      tabWidths = new ConcurrentBag<int>();
      allFoundOptions = new ConcurrentBag<List<IFormatOption>>();
      customInput = new ConcurrentBag<string>() { "TabWidth", "ColumnLimit" };
      defaultLock = new object();
    }

    #endregion

    #region Public Methods 

    public async Task<(EditorStyles matchedStyle, List<IFormatOption> matchedOptions)> DetectStyleOptionsAsync(List<string> filesContent, CancellationToken cancelToken)
    {
      this.filesContent = filesContent;
      await DetectAsync(cancelToken);
      var options = AggregateOptions();
      return (detectedStyle, options);
    }

    #endregion

    #region Private Methods

    private async Task DetectAsync(CancellationToken cancelToken)
    {
      await Task.WhenAll(filesContent.Select(e => CalculateColumTabAsync(e, cancelToken)));
      await Task.WhenAll(filesContent.Select(e => DetectFileStyleAsync(e, cancelToken)));
      detectedStyle = GetStyleByLevenshtein(detectedPredefinedStyles);
      await Task.WhenAll(filesContent.Select(e => DetectFileOptionsAsync(e, detectedStyle, cancelToken)));
    }

    private async Task DetectFileStyleAsync(string content, CancellationToken cancelToken)
    {
      await Task.Run(() =>
      {
        foreach (EditorStyles style in Enum.GetValues(typeof(EditorStyles)))
        {
          if (style == EditorStyles.Custom) continue;
          var levenshtein = GetLevenshteinAfterFormat(content, style, FormatOptionsProvider.GetDefaultOptionsForStyle(style));
          lock (defaultLock)
          {
            if (detectedPredefinedStyles.ContainsKey(style))
            {
              detectedPredefinedStyles[style] += levenshtein;
            }
            else
            {
              detectedPredefinedStyles.Add(style, levenshtein);
            }
          }
        }
      }, cancelToken);
    }

    private List<IFormatOption> AggregateOptions()
    {
      var defaultOptions = FormatOptionsProvider.GetDefaultOptionsForStyle(detectedStyle);
      for (int i = 0; i < defaultOptions.Count; i++)
      {
        var toggleChanged = (0, ToggleValues.False);
        var inputChanged = new Dictionary<string, int>();
        var multipleToggleChanged = false;
        foreach (var option in allFoundOptions)
        {
          switch (option[i])
          {
            case FormatOptionToggleModel foundToggleModel:
              var defaultToggle = (FormatOptionToggleModel)defaultOptions[i];
              if (foundToggleModel.BooleanCombobox != defaultToggle.BooleanCombobox)
              {
                toggleChanged.Item1++;
                toggleChanged.Item2 = foundToggleModel.BooleanCombobox;
              }
              break;
            case FormatOptionInputModel foundInputModel:
              var defaultInput = (FormatOptionInputModel)defaultOptions[i];
              if (foundInputModel.Input != defaultInput.Input)
              {
                if (inputChanged.ContainsKey(foundInputModel.Input))
                {
                  inputChanged[foundInputModel.Input]++;
                }
                else
                {
                  inputChanged.Add(foundInputModel.Input, 1);
                }
              }
              break;
            case FormatOptionMultipleToggleModel foundMultipleToggleModel:
              if (multipleToggleChanged) break;
              var defaultToggleFlags = ((FormatOptionMultipleToggleModel)defaultOptions[i]).ToggleFlags;
              var toggleflags = foundMultipleToggleModel.ToggleFlags;
              for (int j = 0; j < defaultToggleFlags.Count; j++)
              {
                if (toggleflags[j].Value != defaultToggleFlags[j].Value)
                {
                  multipleToggleChanged = true;
                  foundMultipleToggleModel.IsModifed = true;
                  defaultOptions[i] = foundMultipleToggleModel;
                  break;
                }
              }
              break;
            default:
              break;
          }
        }
        if (toggleChanged.Item1 > 0)
        {
          var optionToggle = (FormatOptionToggleModel)defaultOptions[i];
          optionToggle.BooleanCombobox = toggleChanged.Item2;
          optionToggle.IsModifed = true;
          continue;
        }

        if (inputChanged.Count > 0)
        {
          var input = inputChanged.OrderBy(e => e.Value).Last();
          var inputToogle = (FormatOptionInputModel)defaultOptions[i];
          inputToogle.Input = input.Key;
          inputToogle.IsModifed = true;
        }
      }
      return defaultOptions;
    }

    private async Task DetectFileOptionsAsync(string content, EditorStyles style, CancellationToken cancelToken)
    {
      await Task.Run(() =>
      {
        var formatOptions = FormatOptionsProvider.GetDefaultOptionsForStyle(style);
        foreach (var option in formatOptions)
        {
          if (cancelToken.IsCancellationRequested) break;
          SetFormatOption(option, content, style, formatOptions);
        }
        allFoundOptions.Add(formatOptions);
      }, cancelToken);
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
      if (FormatOptionsInputValues.inputValues.ContainsKey(inputModel.Name.Trim()) == false) return;

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
          // cake
          inputModel.Input = item;
          var levenshtein = GetLevenshteinAfterFormat(input, formatStyle, formatOptions);
          inputValuesLevenshtein.Add(item, levenshtein);
        }

        var inputValue = inputValuesLevenshtein.OrderBy(e => e.Value).First();
        inputModel.Input = inputValue.Value == inputValuesLevenshtein[previousInput] ?
                                      previousInput : inputValue.Key;
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
      var test = formatOption.Name;
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

    private async Task CalculateColumTabAsync(string content, CancellationToken cancelToken)
    {
      await Task.Run(() =>
      {
        var lines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
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
      }, cancelToken);
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
