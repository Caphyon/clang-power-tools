using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerTools
{
  public class SettingsViewModelProvider
  {
    public static CompilerSettingsViewModel CompilerSettingsViewModel { get; set; }
    public static FormatSettingsViewModel FormatSettingsViewModel { get; set; }
    public static TidySettingsViewModel TidySettingsViewModel { get; set; }


    public void RefreshCompilerSettingsViewModel()
    {
      CompilerSettingsViewModel.WarningsAsErrors = SettingsModelProvider.CompilerSettings.WarningsAsErrors;
    }
  }
}
