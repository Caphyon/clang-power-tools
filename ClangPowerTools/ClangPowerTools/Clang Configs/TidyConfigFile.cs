using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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

      //Checks line
      paramaterName = "Checks:";
      CreateChecks(paramaterName);

      //Treat warnings as errors line
      paramaterName = "WarningsAsErrors:";
      bool treatWarningsAsErrors = SettingsProvider.GeneralSettings.TreatWarningsAsErrors;
      CreateOutputLine(paramaterName, treatWarningsAsErrors, false);

      //Format after tidy line
      paramaterName = "FormatAfterTidy:";
      bool formatAfterTidy = SettingsProvider.GeneralSettings.TreatWarningsAsErrors;
      CreateOutputLine(paramaterName, treatWarningsAsErrors, false);

      //Continue on error
      paramaterName = "WarningsAsErrors:";
      bool continueOnError = SettingsProvider.GeneralSettings.TreatWarningsAsErrors;
      CreateOutputLine(paramaterName, continueOnError, false);

      //Header filter line
      string headerFilter = SettingsProvider.TidySettings.HeaderFilter.HeaderFilters;
      paramaterName = "HeaderFilterRegex:";
      CreateHeaderFilterOutputLine(paramaterName, headerFilter, true);

      //Format style line
      string formatStyle = SettingsProvider.ClangFormatSettings.Style.Value.ToString();
      paramaterName = "FormatStyle:";
      CreateOutputLine(paramaterName, formatStyle, true);

      //User line
      paramaterName = "User:";
      CreateOutputLine(paramaterName, Environment.UserName, false);

      return tidyConfigOutput;
    }

    private void CreateChecks(string paramaterName)
    {
      ClangTidyUseChecksFrom clangTidyUseChecksFrom = SettingsProvider.TidySettings.UseChecksFrom.Value;
      if (clangTidyUseChecksFrom == ClangTidyUseChecksFrom.CustomChecks)
      {
        CreateCustomChecksOutputLine(paramaterName, SettingsProvider.TidyCustomCheckes.TidyChecks, true);
      }
      else if (clangTidyUseChecksFrom == ClangTidyUseChecksFrom.PredefinedChecks)
      {
        CreateOutputLine(paramaterName, GetPredefinedChecks(SettingsProvider.TidyPredefinedChecks), true);
      }
      else
      {
        CreateOutputLine(paramaterName, "", true);
      }
    }

    private string GetPredefinedChecks(ClangTidyPredefinedChecksOptionsView predefinedChecksSettings)
    {
      StringBuilder predefinedChecks = new StringBuilder();
      PropertyInfo[] properties = predefinedChecksSettings.GetType().GetProperties();

      foreach (var item in properties)
      {
        var attribute = item.GetCustomAttribute(typeof(DisplayNameAttribute)) as DisplayNameAttribute;
        bool? value = item.GetValue(predefinedChecksSettings) as bool?;

        if (value != null && value == true && attribute != null)
        {
          predefinedChecks.Append(attribute.DisplayName + ",");
        }
      }

      return predefinedChecks.ToString().TrimEnd(',');
    }

    private string CreateLine<T>(string name, int nameLength, T value, bool hasQuotationMark)
    {
      if (hasQuotationMark)
      {
        return name.PadRight(maxNameLength - nameLength + nameLength, ' ') + "'" + value + "'";
      }

      return name.PadRight(maxNameLength - nameLength + nameLength, ' ') + value.ToString().ToLower();
    }

    private void CreateOutputLine<T>(string paramaterName, T formatStyle, bool hasQuotationMark)
    {
      tidyConfigOutput.AppendLine(CreateLine(paramaterName, paramaterName.Length, formatStyle, hasQuotationMark));
    }

    private void CreateCustomChecksOutputLine(string paramaterName, string customChecks, bool hasQuotationMark)
    {
      if (customChecks.Length < 1)
      {
        tidyConfigOutput.AppendLine(CreateLine(paramaterName, paramaterName.Length, "", hasQuotationMark));
      }
      else
      {
        tidyConfigOutput.AppendLine(CreateLine(paramaterName, paramaterName.Length, customChecks, hasQuotationMark));
      }
    }

    private void CreateHeaderFilterOutputLine(string paramaterName, string headerFilter, bool hasQuotationMark)
    {
      if (headerFilter.Length < 1)
      {
        tidyConfigOutput.AppendLine(CreateLine(paramaterName, paramaterName.Length, "none", false));
      }
      else if (headerFilter == "Corresponding Header")
      {
        tidyConfigOutput.AppendLine(CreateLine(paramaterName, paramaterName.Length, "_", hasQuotationMark));
      }
      else
      {
        tidyConfigOutput.AppendLine(CreateLine(paramaterName, paramaterName.Length, headerFilter, hasQuotationMark));
      }
    }
  }
}
