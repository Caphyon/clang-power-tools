using ClangPowerTools.MVVM.Interfaces;

namespace ClangPowerTools.MVVM.Models
{
  public class FormatOptionModel : IFormatOption
  {
    public string Name { get; set; }
    public string Description { get; set; }
    public string Input { get; set; } = string.Empty;
  }
}
