using ClangPowerTools.MVVM.Interfaces;
using System.Collections.ObjectModel;

namespace ClangPowerTools.MVVM.Models
{
  public class FormatOptionModel : IFormatOption
  {
    public string Name { get; set; }
    public string Description { get; set; }
    public string Paramater { get; set; } = string.Empty;
    public string Input { get; set; } = string.Empty;
    public ObservableCollection<string> BooleanComboboxValues { get; set; } = new ObservableCollection<string>() { "true", "false" };
    public bool HasBooleanCombobox { get; } = false;
    public bool HasInputTextBox { get; } = true;
    public bool IsEnabled { get; set; } = false;
  }
}
