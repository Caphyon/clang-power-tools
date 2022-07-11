using ClangPowerToolsShared.MVVM.Constants;
using System.ComponentModel;

namespace ClangPowerToolsShared.MVVM.Models.ToolWindowModels
{
  public class CustomMatchesModel : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    private string matches = string.Empty;
    private string visibility = string.Empty;

    public CustomMatchesModel()
    {
      visibility = UIElementsConstants.Hidden;
    }

    public string Matches
    {
      get { return matches; }
      set
      {
        matches = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Matches"));
      }
    }

    public string Visibility
    {
      get { return visibility; }
      set
      {
        visibility = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Visibility"));
      }
    }

    public string Details { get; } = "Details\n";

    public void Hide()
    {
      visibility = UIElementsConstants.Hidden;
    }

    public void Show()
    {
      visibility = UIElementsConstants.Visibile;
    }
  }
}
