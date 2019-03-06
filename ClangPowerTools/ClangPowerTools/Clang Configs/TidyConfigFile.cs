using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerTools
{
  public class TidyConfigFile
  {
    // Create StringBuilder to be written in the .clang-tidy file
    private StringBuilder tidyConfigOutput = new StringBuilder();

    // Get all the tidy settings
    private static ClangTidyPredefinedChecksOptionsView predefinedChecksSettings = SettingsProvider.TidyPredefinedChecks;
    private int maxNameLength = 19;

    public StringBuilder CreateOutput()
    {
      string paramaterName = "";
      tidyConfigOutput.AppendLine("---");

      //Custom checks line
      string customChecks = SettingsProvider.TidyCustomCheckes.TidyChecks;
      paramaterName = "Checks:";
      if (customChecks.Length < 1)
      {
        tidyConfigOutput.AppendLine(CreateLine(paramaterName, paramaterName.Length, ""));
      }
      else
      {
        tidyConfigOutput.AppendLine(CreateLine(paramaterName, paramaterName.Length, customChecks));
      }

      //Treat warnings as errors line
      paramaterName = "WarningsAsErrors:";
      bool treatWarningsAsErrors = SettingsProvider.GeneralSettings.TreatWarningsAsErrors;
      if (!treatWarningsAsErrors)
      {
        tidyConfigOutput.AppendLine(CreateLine(paramaterName, paramaterName.Length, treatWarningsAsErrors));
      }
      else
      {
        tidyConfigOutput.AppendLine(CreateLine(paramaterName, paramaterName.Length, treatWarningsAsErrors));
      }

      //Header filter line
      string headerFilter = SettingsProvider.TidySettings.HeaderFilter.HeaderFilters;
      paramaterName = "HeaderFilterRegex:";
      if (headerFilter.Length < 1)
      {
        tidyConfigOutput.AppendLine(CreateLine(paramaterName, paramaterName.Length, ""));
      }
      else
      {
        tidyConfigOutput.AppendLine(CreateLine(paramaterName, paramaterName.Length, headerFilter));
      }

      //Format style line
      string formatStyle = SettingsProvider.ClangFormatSettings.Style.Value.ToString();
      paramaterName = "FormatStyle:";
      tidyConfigOutput.AppendLine(CreateLine(paramaterName, paramaterName.Length, formatStyle));


      GetPredefinedChecks();

      return tidyConfigOutput;
    }

    private string CreateLine(string name, int nameLength, string value)
    {
      return name.PadRight(maxNameLength - nameLength + nameLength, ' ') + "'" + value + "'";
    }

    private string CreateLine(string name, int nameLength, bool value)
    {
      return name.PadRight(maxNameLength - nameLength + nameLength, ' ') + value.ToString().ToLower();
    }

    private void GetPredefinedChecks()
    {
      StringBuilder stringBuilder = new StringBuilder("CheckOptions:");
      foreach (var item in predefinedChecksSettings.GetType().GetProperties())
      {
       // stringBuilder.AppendLine(item.getta);
      }
    }
  }
}
