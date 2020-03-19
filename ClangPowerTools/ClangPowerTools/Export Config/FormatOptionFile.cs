using ClangPowerTools.MVVM.Interfaces;
using ClangPowerTools.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClangPowerTools
{
  public class FormatOptionFile
  {
    public static StringBuilder CreateOutput(List<IFormatOption> formatOptions, EditorStyles style)
    {
      var output = new StringBuilder();
      output.AppendLine("# Format Style Options - Created with Clang Power Tools");
      output.AppendLine("---");
      output.AppendLine("Language: Cpp");

      switch (style)
      {
        case EditorStyles.LLVM:
          output.AppendLine("BasedOnStyle: LLVM");
          break;
        case EditorStyles.Google:
          output.AppendLine("BasedOnStyle: Google");
          break;
        case EditorStyles.Chromium:
          output.AppendLine("BasedOnStyle: Google");
          break;
        case EditorStyles.Mozilla:
          output.AppendLine("BasedOnStyle: Google");
          break;
        case EditorStyles.WebKit:
          output.AppendLine("BasedOnStyle: Google");
          break;
        case EditorStyles.Microsoft:
          output.AppendLine("BasedOnStyle: Google");
          break;
      } 

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

          output.AppendLine(styleOption);
        }
      }
      output.AppendLine("...");


      return output;
    }
  }
}
