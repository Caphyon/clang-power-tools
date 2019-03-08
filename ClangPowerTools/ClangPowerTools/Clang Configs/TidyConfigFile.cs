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
    // Max length used to add space padding for the paramater name in a line
    private int maxNameLength = 19;

    public StringBuilder CreateOutput()
    {
      string paramaterName = "";
      tidyConfigOutput.AppendLine("---");

      //Custom checks line
      string customChecks = SettingsProvider.TidyCustomCheckes.TidyChecks;
      paramaterName = "Checks:";
      CreateCustomChecksLine(paramaterName, customChecks);

      //Treat warnings as errors line
      paramaterName = "WarningsAsErrors:";
      bool treatWarningsAsErrors = SettingsProvider.GeneralSettings.TreatWarningsAsErrors;
      CreateTreatWarningAsErrorsLine(paramaterName, treatWarningsAsErrors);

      //Header filter line
      string headerFilter = SettingsProvider.TidySettings.HeaderFilter.HeaderFilters;
      paramaterName = "HeaderFilterRegex:";
      CreateHeaderFilterLine(paramaterName, headerFilter);

      //Format style line
      string formatStyle = SettingsProvider.ClangFormatSettings.Style.Value.ToString();
      paramaterName = "FormatStyle:";
      CreateStyleLine(paramaterName, formatStyle);

      //Predifined checks line
      ClangTidyPredefinedChecksOptionsView predefinedChecksSettings = SettingsProvider.TidyPredefinedChecks;
      paramaterName = "CheckOptions:";
      CreatePredefinedChecksLine(paramaterName, GetPredefinedChecks(predefinedChecksSettings));

      return tidyConfigOutput;
    }

    private string GetPredefinedChecks(ClangTidyPredefinedChecksOptionsView predefinedChecksSettings)
    {
      StringBuilder predefinedChecks = new StringBuilder("CheckOptions:");
      foreach (var item in predefinedChecksSettings.GetType().GetProperties())
      {
        string name = item.Name;
        bool? value = item.GetValue(predefinedChecksSettings) as bool?;

        if (value!= null && value == true)
        {        
          predefinedChecks.Append(name + "  ").Append(value).AppendLine();
        }
      }
      return predefinedChecks.ToString();
    }

    private string CreateLine(string name, int nameLength, string value)
    {
      return name.PadRight(maxNameLength - nameLength + nameLength, ' ') + "'" + value + "'";
    }

    private string CreateLine(string name, int nameLength, bool value)
    {
      return name.PadRight(maxNameLength - nameLength + nameLength, ' ') + value.ToString().ToLower();
    }

    private void CreateStyleLine(string paramaterName, string formatStyle)
    {
      tidyConfigOutput.AppendLine(CreateLine(paramaterName, paramaterName.Length, formatStyle));
    }

    private void CreatePredefinedChecksLine(string paramaterName, string formatStyle)
    {
      tidyConfigOutput.AppendLine(CreateLine(paramaterName, paramaterName.Length, formatStyle));
    }

    private void CreateHeaderFilterLine(string paramaterName, string headerFilter)
    {
      if (headerFilter.Length < 1)
      {
        tidyConfigOutput.AppendLine(CreateLine(paramaterName, paramaterName.Length, ""));
      }
      else
      {
        tidyConfigOutput.AppendLine(CreateLine(paramaterName, paramaterName.Length, headerFilter));
      }
    }

    private void CreateTreatWarningAsErrorsLine(string paramaterName, bool treatWarningsAsErrors)
    {
      if (!treatWarningsAsErrors)
      {
        tidyConfigOutput.AppendLine(CreateLine(paramaterName, paramaterName.Length, treatWarningsAsErrors));
      }
      else
      {
        tidyConfigOutput.AppendLine(CreateLine(paramaterName, paramaterName.Length, treatWarningsAsErrors));
      }
    }

    private void CreateCustomChecksLine(string paramaterName, string customChecks)
    {
      if (customChecks.Length < 1)
      {
        tidyConfigOutput.AppendLine(CreateLine(paramaterName, paramaterName.Length, ""));
      }
      else
      {
        tidyConfigOutput.AppendLine(CreateLine(paramaterName, paramaterName.Length, customChecks));
      }
    }
  }
}
