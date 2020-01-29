using ClangPowerTools.MVVM.Interfaces;

namespace ClangPowerTools.MVVM.Models
{
  class FormatOptionToggleModel : IFormatOption
  {
    public string Name { get; set; }
    public string Description { get; set; }
    public string Paramater { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = false;
    public bool HasToogleButton { get; } = true;
    public bool HasTextBox { get; } = false;
  }
}
