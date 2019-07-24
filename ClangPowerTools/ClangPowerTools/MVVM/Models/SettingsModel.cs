using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerTools.MVVM.Models
{
  public class SettingsModel
  {
    public CompilerSettingsModel CompilerSettings { get; set; }

    public FormatSettingsModel FormatSettings { get; set; }
  }
}
