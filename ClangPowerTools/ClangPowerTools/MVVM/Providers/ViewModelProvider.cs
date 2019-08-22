using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerTools
{
  public class ViewModelProvider
  {
    public static CompilerSettingsViewModel CompilerSettingsViewModel { get; set; } = new CompilerSettingsViewModel();
    public static FormatSettingsViewModel FormatSettingsViewModel { get; set; } = new FormatSettingsViewModel();
    public static TidySettingsViewModel TidySettingsViewModel { get; set; } = new TidySettingsViewModel();
  }
}
