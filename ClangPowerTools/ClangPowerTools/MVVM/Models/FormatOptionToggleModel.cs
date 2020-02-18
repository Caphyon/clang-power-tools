using ClangPowerTools.MVVM.Interfaces;
using System.Collections.ObjectModel;

namespace ClangPowerTools.MVVM.Models
{
  class FormatOptionToggleModel : IFormatOption
  {
    public string Name { get; set; }
    public string Description { get; set; }
    public string Paramater { get; set; } = string.Empty;
    public ToggleValues BooleanCombobox { get; set; }
    public bool HasBooleanCombobox { get; } = true;
    public bool HasInputTextBox { get; } = false;
    public bool IsEnabled { get; set; } = false;
  }
}
