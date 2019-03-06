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
    private string customChecks = SettingsProvider.TidyCustomCheckes.TidyChecks;
    private ClangTidyPredefinedChecksOptionsView tidyPredefinedChecksSettings = SettingsProvider.TidyPredefinedChecks;
    private string predefinnedChecks = SettingsProvider.TidySettings.HeaderFilter.HeaderFilters;
    private string formatStyle = SettingsProvider.ClangFormatSettings.Style.Value.ToString();
    private bool treatWarningsAsErrors = SettingsProvider.GeneralSettings.TreatWarningsAsErrors;






    public StringBuilder CreateOutput()
    {
      tidyConfigOutput.AppendLine("---");
      if(customChecks.Length < 1)
      {
        customChecks = "''";
      }
      tidyConfigOutput.AppendLine("Checks:" + "'" + customChecks + "'");

      if (!treatWarningsAsErrors)
      {
        tidyConfigOutput.AppendLine("WarningsAsErrors:" + "''");
      }
      else
      {
        tidyConfigOutput.AppendLine("WarningsAsErrors:" + "'" + treatWarningsAsErrors.ToString() + "'" );
      }

      tidyConfigOutput.AppendLine("FormatStyle:" + "'" + formatStyle + "'" );

      return tidyConfigOutput;
    }



    private void GetPredefinedChecks()
    {
      foreach (var item in tidyPredefinedChecksSettings.GetType().GetProperties())
      {
       
      }
    }
  }
}
