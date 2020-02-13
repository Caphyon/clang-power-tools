using ClangPowerTools.MVVM.Models;
using System.Text;

namespace ClangPowerTools
{
  public class FormatOptionFile
  {
    public static StringBuilder CreateOutput()
    {
      var output = new StringBuilder();
      output.AppendLine("# Configurable Format Style Options - Created with Clang Power Tools");
      output.AppendLine("---");
      output.AppendLine("Language: Cpp");

      foreach (var item in FormatOptionsData.FormatOptions)
      {
        var styleOption = string.Empty;
        if (item is FormatOptionToggleModel)
        {
          var option = item as FormatOptionToggleModel;
          styleOption = string.Concat(option.Name, ": ", option.IsEnabled.ToString().ToLower());
        }
        else if (item is FormatOptionModel)
        {
          var option = item as FormatOptionModel;
          if (string.IsNullOrEmpty(option.Input)) continue;
          styleOption = string.Concat(option.Name, ": ", option.Input);
        }

        output.AppendLine(styleOption);
      }

      return output;
    }
  }
}
