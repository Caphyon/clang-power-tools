using ClangPowerTools.MVVM.Interfaces;
using ClangPowerTools.MVVM.Models;
using System.Collections.Generic;
using System.Text;

namespace ClangPowerTools
{
  public class FormatOptionFile
  {
    public static StringBuilder CreateOutput(List<IFormatOption> formatOptions, EditorStyles style)
    {
      List<IFormatOption> options;
      var output = new StringBuilder();
      output.AppendLine("# Format Style Options - Created with Clang Power Tools");
      output.AppendLine("---");
      output.AppendLine("Language: Cpp");

      switch (style)
      {
        case EditorStyles.LLVM:
          output.AppendLine("BasedOnStyle: LLVM");
          options = CompareFormatOptions(formatOptions, new FormatOptionsData().FormatOptions);
          AddActiveOptionToFile(options, output);
          break;
        case EditorStyles.Google:
          output.AppendLine("BasedOnStyle: Google");
          options = CompareFormatOptions(formatOptions, new FormatOptionsGoogleData().FormatOptions);
          AddActiveOptionToFile(options, output);
          break;
        case EditorStyles.Chromium:
          output.AppendLine("BasedOnStyle: Chromium");
          options = CompareFormatOptions(formatOptions, new FormatOptionsChromiumData().FormatOptions);
          AddActiveOptionToFile(options, output);
          break;
        case EditorStyles.Mozilla:
          output.AppendLine("BasedOnStyle: Mozilla");
          options = CompareFormatOptions(formatOptions, new FormatOptionsMozillaData().FormatOptions);
          AddActiveOptionToFile(options, output);
          break;
        case EditorStyles.WebKit:
          output.AppendLine("BasedOnStyle: WebKit");
          options = CompareFormatOptions(formatOptions, new FormatOptionsWebKitData().FormatOptions);
          AddActiveOptionToFile(options, output);
          break;
        case EditorStyles.Microsoft:
          output.AppendLine("BasedOnStyle: Microsoft");
          options = CompareFormatOptions(formatOptions, new FormatOptionsMicrosoftData().FormatOptions);
          AddActiveOptionToFile(options, output);
          break;
        default:
          AddActiveOptionToFile(formatOptions, output);
          break;
      }

      output.AppendLine("...");

      return output;
    }

    private static List<IFormatOption> CompareFormatOptions(List<IFormatOption> currentOptions, List<IFormatOption> defaultOptions)
    {

      for (int i = 0; i < currentOptions.Count; i++)
      {
        if (currentOptions[i] is FormatOptionToggleModel)
        {
          var currentOption = currentOptions[i] as FormatOptionToggleModel;
          var defaultOption = defaultOptions[i] as FormatOptionToggleModel;
          if (currentOption.BooleanCombobox == defaultOption.BooleanCombobox)
          {
            currentOptions[i].IsEnabled = false;
          }
        }
        else if (currentOptions[i] is FormatOptionInputModel)
        {
          var currentOption = currentOptions[i] as FormatOptionInputModel;
          var defaultOption = defaultOptions[i] as FormatOptionInputModel;
          if (string.Compare(currentOption.Input, defaultOption.Input) == 0 || string.IsNullOrEmpty(currentOption.Input))
          {
            currentOptions[i].IsEnabled = false;
          }
        }
        else if (currentOptions[i] is FormatOptionMultipleInputModel)
        {
          var currentOption = currentOptions[i] as FormatOptionMultipleInputModel;
          var defaultOption = defaultOptions[i] as FormatOptionMultipleInputModel;
          if (string.Compare(currentOption.MultipleInput, defaultOption.MultipleInput) == 0 || string.IsNullOrEmpty(currentOption.MultipleInput))
          {
            currentOptions[i].IsEnabled = false;
          }
        }
      }
      return currentOptions;
    }

    private static void AddActiveOptionToFile(List<IFormatOption> formatOptions, StringBuilder output)
    {
      foreach (var item in formatOptions)
      {
        if (item.IsEnabled)
        {
          var styleOption = string.Empty;
          if (item is FormatOptionToggleModel)
          {
            var option = item as FormatOptionToggleModel;
            styleOption = string.Concat(option.Name, ": ", option.BooleanCombobox.ToString().ToLower());
          }
          else if (item is FormatOptionInputModel)
          {
            var option = item as FormatOptionInputModel;
            if (string.IsNullOrEmpty(option.Input)) continue;
            styleOption = string.Concat(option.Name, ": ", option.Input);
          }
          else if (item is FormatOptionMultipleInputModel)
          {
            var option = item as FormatOptionMultipleInputModel;
            if (string.IsNullOrEmpty(option.MultipleInput)) continue;
            styleOption = string.Concat(option.Name, ": \r\n", option.MultipleInput);
          }

          output.AppendLine(styleOption);
        }
      }
    }
  }
}
