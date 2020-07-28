using ClangPowerTools.MVVM.Interfaces;
using ClangPowerTools.MVVM.Models;
using ClangPowerTools.MVVM.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClangPowerTools.MVVM.Controllers
{
  public class DiffController
  {
    #region Members

    public EventHandler ClosedWindow;

    private bool windowClosed;
    private readonly StyleFormatter formatter;
    private readonly Dictionary<EditorStyles, List<IFormatOption>> styles;

    private EditorStyles formatStyle;
    private List<IFormatOption> formatOptions;
    private string editorInput;
    private readonly Action CreateFormatFile;

    #endregion


    #region Constructor

    public DiffController(Action CreateFormatFile)
    {
      formatter = new StyleFormatter();
      styles = CreateStyles();
      ClosedWindow += CloseLoadingView;
      windowClosed = false;
      this.CreateFormatFile = CreateFormatFile;
    }

    private void CloseLoadingView(object sender, EventArgs e)
    {
      windowClosed = true;
      ClosedWindow -= CloseLoadingView;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Get the found EditorStyle and FormatOptions
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public async Task<(EditorStyles matchedStyle, List<IFormatOption> matchedOptions)> GetFormatOptionsAsync(string text)
    {
      editorInput = text;

      await Task.Run(() =>
      {
        var (matchedStyle, matchedOptions) = GetClosestDefaultStyle(text);
        formatStyle = matchedStyle;
        formatOptions = matchedOptions;

        foreach (var option in formatOptions)
        {
          if (windowClosed) return;
          SetFormatOption(option);
        }
      });

      return (formatStyle, formatOptions);
    }

    /// <summary>
    /// Display the diffs in an html format after GetFormatOptionsAsync
    /// </summary>
    /// <returns></returns>
    public async Task ShowHtmlAfterDiffAsync(string formatOptionFile)
    {
      string html = string.Empty;
      await Task.Run(() =>
      {
        var formattedText = formatter.FormatText(editorInput, formatOptions, formatStyle);
        var diffMatchPatchWrapper = new DiffMatchPatchWrapper();
        diffMatchPatchWrapper.Diff(editorInput, formattedText);
        diffMatchPatchWrapper.CleanupSemantic();
        html = diffMatchPatchWrapper.DiffAsHtml();
      });

      var diffWindow = new DiffWindow(html, formatOptionFile, CreateFormatFile);
      diffWindow.Show();
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

      var minLevenshtein = GetIndexOfSmallestLevenshtein(levenshteinDiffs);
      var matchedStyle = styles.ElementAt(minLevenshtein);
      return (matchedStyle.Key, matchedStyle.Value);
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
        default:
          break;
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
      inputValuesLevenshtein.Add(ToggleValues.False, GetLevenshteinAfterOptionChange());

      modelToggle.BooleanCombobox = ToggleValues.True;
      inputValuesLevenshtein.Add(ToggleValues.True, GetLevenshteinAfterOptionChange());

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
        if (windowClosed) return;
        inputModel.Input = item;
        inputValuesLevenshtein.Add(item, GetLevenshteinAfterOptionChange());
      }

      var inputValue = inputValuesLevenshtein.OrderBy(e => e.Value).First();

      inputModel.Input = inputValue.Value == inputValuesLevenshtein[previousInput] ?
                         previousInput : inputValue.Key;
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

    #endregion
  }
}
