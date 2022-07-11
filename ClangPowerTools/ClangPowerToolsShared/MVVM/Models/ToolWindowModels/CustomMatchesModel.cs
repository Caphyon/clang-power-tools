using System.ComponentModel;

namespace ClangPowerToolsShared.MVVM.Models.ToolWindowModels
{
  public class CustomMatchesModel : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    private string matches = string.Empty;

    public string Matches
    {
      get { return matches; }
      set
      {
        matches = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Matches"));
      }
    }

    public string Details { get; } = "Details\n";
  }
}
