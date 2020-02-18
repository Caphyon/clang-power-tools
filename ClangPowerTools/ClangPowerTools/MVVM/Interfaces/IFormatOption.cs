using System.Collections.ObjectModel;

namespace ClangPowerTools.MVVM.Interfaces
{
  public interface IFormatOption
  {
    public string Name { get; set; }
    public string Description { get; set; }
    public string Paramater { get; set; }
    public ObservableCollection<string> BooleanComboboxValues { get; set; }
    public bool HasBooleanCombobox { get; }
    public bool HasInputTextBox { get; }
    public bool IsEnabled { get; set; }
  }
}
