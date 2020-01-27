using ClangPowerTools.MVVM.Interfaces;

namespace ClangPowerTools.MVVM.Models
{
  class FormatOptionToggleModel : IFormatOption
  {
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsEnabled { get; set; } = false;
    public bool IsToogleButton { get; set; } = true;
    public bool IsTextBox { get; set; } = false;
  }
}
