using ClangPowerTools.Helpers;
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
    private CancellationTokenSource cancellationSource;
    private bool stopDetector;
    private readonly Dictionary<EditorStyles, int> detectedPredefinedStyles;
    private readonly ConcurrentBag<string> customInput;
    private readonly ConcurrentBag<int> columnLimits;
    private readonly ConcurrentBag<int> tabWidths;
    private readonly ConcurrentBag<List<IFormatOption>> allFoundOptions;
    private readonly object defaultLock;

    #endregion

    #region Properties

    public bool StopDetector
    {
      get
      {
        return stopDetector;
      }
      set
      {
        stopDetector = value;
        if (stopDetector)
        {
          cancellationSource.Cancel();
        }
      }
    }

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
      stopDetector = false;
    }

    #endregion

    #region Public Methods 

    public async Task<(EditorStyles matchedStyle, List<IFormatOption> matchedOptions)> DetectStyleOptionsAsync(string input)
    {
      filesContent.Add(input);
      await DetectAsync();
      var options = AggregateOptions();
      return (detectedStyle, options);
    }

    public async Task<(EditorStyles matchedStyle, List<IFormatOption> matchedOptions)> DetectStyleOptionsAsync(List<string> filePaths)
    {
      filesContent = FileSystem.ReadContentFromMultipleFiles(filePaths, Environment.NewLine);
      await DetectAsync();
      var options = AggregateOptions();
      return (detectedStyle, options);
    }

    #endregion

    #region Private Methods

    private async Task DetectAsync()
    {
      cancellationSource = new CancellationTokenSource();
      CancellationToken cancelToken = cancellationSource.Token;
      try
      {
        await Task.WhenAll(filesContent.Select(e => CalculateColumTabAsync(e, cancelToken)));
        await Task.WhenAll(filesContent.Select(e => DetectFileStyleAsync(e, cancelToken)));
        detectedStyle = GetStyleByLevenshtein(detectedPredefinedStyles);
        await Task.WhenAll(filesContent.Select(e => DetectFileOptionsAsync(e, detectedStyle, cancelToken)));
      }
      catch (OperationCanceledException)
      {
      }
      finally
      {
        cancellationSource.Dispose();
      }
    }

    private async Task DetectFileStyleAsync(string content, CancellationToken cancelToken)
    {
      await Task.Run(() =>
      {
        foreach (EditorStyles style in Enum.GetValues(typeof(EditorStyles)))
        {
          if (style == EditorStyles.Custom) continue;
          var levenshtein = GetLevenshteinAfterFormat(content, style, GetDefaultOptionsForStyle(style));
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
      var fileCount = filesContent.Count;
      var defaultOptions = GetDefaultOptionsForStyle(detectedStyle);
      for (int i = 0; i < defaultOptions.Count; i++)
      {
        var toggleChanged = (0, ToggleValues.False);
        var inputChanged = new Dictionary<string, int>();
        var multipleToggleChanged = false;
        foreach (var option in allFoundOptions)
        {
          switch (option[i])
          {
            case FormatOptionToggleModel toggleModel:
              var defaultToggle = (FormatOptionToggleModel)defaultOptions[i];
              if (toggleModel.BooleanCombobox != defaultToggle.BooleanCombobox)
              {
                toggleChanged.Item1++;
                toggleChanged.Item2 = toggleModel.BooleanCombobox;
              }
              break;
            case FormatOptionInputModel inputModel:
              var defaultInput = (FormatOptionInputModel)defaultOptions[i];
              if (inputModel.Input != defaultInput.Input)
              {
                if (inputChanged.ContainsKey(inputModel.Input))
                {
                  inputChanged[inputModel.Input]++;
                }
                else
                {
                  inputChanged.Add(inputModel.Input, 1);
                }
              }
              break;
            case FormatOptionMultipleToggleModel multipleToggleModel:
              if (multipleToggleChanged) break;
              var defaultToggleFlags = ((FormatOptionMultipleToggleModel)defaultOptions[i]).ToggleFlags;
              var toggleflags = multipleToggleModel.ToggleFlags;
              for (int j = 0; j < defaultToggleFlags.Count; j++)
              {
                if (toggleflags[j].Value != defaultToggleFlags[j].Value)
                {
                  multipleToggleChanged = true;
                  defaultOptions[i] = multipleToggleModel;
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
          ((FormatOptionToggleModel)defaultOptions[i]).BooleanCombobox = toggleChanged.Item2;
          continue;
        }

        if (inputChanged.Count > 0)
        {
          var input = inputChanged.OrderBy(e => e.Value).Last();
          ((FormatOptionInputModel)defaultOptions[i]).Input = input.Key;
        }
      }
      return defaultOptions;
    }

    private async Task DetectFileOptionsAsync(string content, EditorStyles style, CancellationToken cancelToken)
    {
      await Task.Run(() =>
      {
        var formatOptions = GetDefaultOptionsForStyle(style);
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
          inputModel.Input = item;
          inputValuesLevenshtein.Add(item, GetLevenshteinAfterFormat(input, formatStyle, formatOptions));
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

    private List<IFormatOption> GetDefaultOptionsForStyle(EditorStyles style)
    {
      switch (style)
      {
        case EditorStyles.Custom:
          break;
        case EditorStyles.LLVM:
          return new FormatOptionsData().FormatOptions;
        case EditorStyles.Google:
          return new FormatOptionsGoogleData().FormatOptions;
        case EditorStyles.Chromium:
          return new FormatOptionsChromiumData().FormatOptions;
        case EditorStyles.Microsoft:
          return new FormatOptionsMicrosoftData().FormatOptions;
        case EditorStyles.Mozilla:
          return new FormatOptionsMozillaData().FormatOptions;
        case EditorStyles.WebKit:
          return new FormatOptionsWebKitData().FormatOptions;
        default:
          break;
      }
      return new FormatOptionsData().FormatOptions;
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
