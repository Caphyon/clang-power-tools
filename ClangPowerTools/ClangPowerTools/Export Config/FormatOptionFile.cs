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
      var optionsToInclude = new List<IFormatOption>();
      for (int i = 0; i < currentOptions.Count; i++)
      {
        // TODO use switch

        if (currentOptions[i] is FormatOptionToggleModel)
        {
          var currentOption = currentOptions[i] as FormatOptionToggleModel;
          var defaultOption = defaultOptions[i] as FormatOptionToggleModel;
          if (currentOption.BooleanCombobox == defaultOption.BooleanCombobox)
          {
            continue;
          }
        }
        else if (currentOptions[i] is FormatOptionInputModel)
        {
          var currentOption = currentOptions[i] as FormatOptionInputModel;
          var defaultOption = defaultOptions[i] as FormatOptionInputModel;
          if (string.Compare(currentOption.Input, defaultOption.Input) == 0 || string.IsNullOrEmpty(currentOption.Input))
          {
            continue;
          }
        }
        else if (currentOptions[i] is FormatOptionMultipleInputModel)
        {
          var currentOption = currentOptions[i] as FormatOptionMultipleInputModel;
          var defaultOption = defaultOptions[i] as FormatOptionMultipleInputModel;
          if (string.Compare(currentOption.MultipleInput, defaultOption.MultipleInput) == 0 || string.IsNullOrEmpty(currentOption.MultipleInput))
          {
            continue;
          }
        }
        else if (currentOptions[i] is FormatOptionMultipleToggleModel)
        {
          var currentOption = currentOptions[i] as FormatOptionMultipleToggleModel;
          var defaultOption = defaultOptions[i] as FormatOptionMultipleToggleModel;

          var toggleFlags = RemoveUnchagedToogleFlags(currentOption.ToggleFlags, defaultOption.ToggleFlags);

          if (toggleFlags.Count == 0)
          {
            continue;
          }
          else
          {
            var formatOptionMultipleToggleModel = new FormatOptionMultipleToggleModel
            {
              ToggleFlags = toggleFlags,
              Name = currentOption.Name
            };
            optionsToInclude.Add(formatOptionMultipleToggleModel);

            continue;
          }
        }
        optionsToInclude.Add(currentOptions[i]);
      }

      return optionsToInclude;
    }

    private static void AddActiveOptionToFile(List<IFormatOption> formatOptions, StringBuilder output)
    {
      foreach (var item in formatOptions)
      {
        if (item.IsEnabled == false) continue;

        var styleOption = string.Empty;
        switch (item)
        {
          case FormatOptionToggleModel option:
            styleOption = string.Concat(option.Name, ": ", option.BooleanCombobox.ToString().ToLower());
            break;
          case FormatOptionInputModel option when string.IsNullOrEmpty(option.Input) == false:
            styleOption = string.Concat(option.Name, ": ", option.Input);
            break;
          case FormatOptionMultipleInputModel option when string.IsNullOrEmpty(option.MultipleInput) == false:
            styleOption = string.Concat(option.Name, ": \r\n", option.MultipleInput);
            break;
          case FormatOptionMultipleToggleModel option:
            styleOption = string.Concat(option.Name, ": \r\n", CreateMultipleToggleFlag(option.ToggleFlags));
            break;
          default:
            break;
        }

        output.AppendLine(styleOption);
      }
    }

    private static List<ToggleModel> RemoveUnchagedToogleFlags(List<ToggleModel> currentOption, List<ToggleModel> defaultOption)
    {
      var modifedFlags = new List<ToggleModel>();
      for (int index = 0; index < currentOption.Count; index++)
      {
        if (currentOption[index].Value != defaultOption[index].Value)
        {
          modifedFlags.Add(currentOption[index]);
        }
      }
      return modifedFlags;
    }

    private static string CreateMultipleToggleFlag(List<ToggleModel> toggleModels)
    {
      var sb = new StringBuilder();

      foreach (var item in toggleModels)
      {
        sb.AppendLine(string.Concat("  ", item.Name, ": ", item.Value.ToString().ToLower()));
      }
      return sb.ToString().TrimEnd();
    }
  }
}

