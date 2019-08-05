using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ClangPowerTools
{
  public class TidyConfigFile
  {
    #region Members

    // Create StringBuilder to be written in the .clang-tidy file
    private StringBuilder tidyConfigOutput = new StringBuilder();

    // Readonly list for paramaters names
    private static readonly List<string> parameterNames = new List<string>()
    {
      "Checks:", "WarningsAsErrors:", "WarningsAsErrors:",
      "HeaderFilterRegex:", "FormatStyle:", "User:"
    };

    // Max length used to add space padding for the paramater name in a line
    private int maxNameLength = 0;
    
    #endregion

    #region Public Methods

    public StringBuilder CreateOutput()
    {
      maxNameLength = parameterNames.OrderByDescending(s => s.Length).First().Length;

      tidyConfigOutput.AppendLine("---");

      //Checks line
      CreateChecksOutputLine(parameterNames.ElementAt(0));

      //Treat warnings as errors line
      string treatWarningsAsErrors = ScriptConstants.kTreatWarningsAsErrors;
      CreateWarningAsErrorsOutputLine(parameterNames.ElementAt(1), treatWarningsAsErrors, true);

      //Header filter line
      string headerFilter = SettingsProvider.TidySettings.HeaderFilter.HeaderFilters;
      CreateHeaderFilterOutputLine(parameterNames.ElementAt(3), headerFilter, true);

      //Format style line
      string formatStyle = SettingsProvider.ClangFormatSettings.Style.ToString();
      CreateOutputLine(parameterNames.ElementAt(4), formatStyle, true);

      //User line
      CreateOutputLine(parameterNames.ElementAt(5), Environment.UserName, false);

      return tidyConfigOutput;
    }

    #endregion

    #region Private Methods 

    private void CreateChecksOutputLine(string paramaterName)
    {
      ClangTidyUseChecksFrom clangTidyUseChecksFrom = SettingsProvider.TidySettings.UseChecksFrom;
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

    private string CreateLine<T>(string propertyName, int nameLength, T value, bool hasQuotationMark)
    {
      if (hasQuotationMark)
      {
        return $"{propertyName.PadRight(maxNameLength - nameLength + nameLength, ' ')} '{value}'";
      }
      return $"{propertyName.PadRight(maxNameLength - nameLength + nameLength, ' ')} {value.ToString().ToLower()}";
    }

    private void CreateOutputLine<T>(string paramaterName, T formatStyle, bool hasQuotationMark)
    {
      tidyConfigOutput.AppendLine(CreateLine(paramaterName, paramaterName.Length, formatStyle, hasQuotationMark));
    }

    private void CreateWarningAsErrorsOutputLine(string paramaterName, string warningsAsErrors, bool hasQuotationMark)
    {
      if (SettingsProvider.GeneralSettings.TreatWarningsAsErrors)
      {
        tidyConfigOutput.AppendLine(CreateLine(paramaterName, paramaterName.Length, warningsAsErrors, hasQuotationMark));
      }
      else
      {
        tidyConfigOutput.AppendLine(CreateLine(paramaterName, paramaterName.Length, string.Empty, hasQuotationMark));
      }
    }

    private void CreateCustomChecksOutputLine(string paramaterName, string customChecks, bool hasQuotationMark)
    {
      if (customChecks.Length < 1)
      {
        tidyConfigOutput.AppendLine(CreateLine(paramaterName, paramaterName.Length, string.Empty, hasQuotationMark));
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
        tidyConfigOutput.AppendLine(CreateLine(paramaterName, paramaterName.Length, ComboBoxConstants.kNone, false));
      }
      else if (headerFilter == ComboBoxConstants.kCorrespondingHeaderName)
      {
        tidyConfigOutput.AppendLine(CreateLine(paramaterName, paramaterName.Length, ComboBoxConstants.kCorrespondingHeaderValue, hasQuotationMark));
      }
      else
      {
        tidyConfigOutput.AppendLine(CreateLine(paramaterName, paramaterName.Length, headerFilter, hasQuotationMark));
      }
    }

    #endregion
  }
}
