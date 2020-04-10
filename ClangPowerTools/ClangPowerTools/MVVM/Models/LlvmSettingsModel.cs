using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerTools
{
  public class LlvmSettingsModel
  {
    public string LlvmSelectedVersion { get; set; } = string.Empty;

    public string PreinstalledLlvmVersion { get; set; } = string.Empty;

    public string PreinstalledLlvmPath { get; set; } = string.Empty;
  }
}
