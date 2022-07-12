using ClangPowerToolsShared.MVVM.Constants;
using ClangPowerToolsShared.MVVM.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;

namespace ClangPowerToolsShared.MVVM.Models.ToolWindowModels
{
  public class CustomMatchesModel : ComponentVisibility, IViewMatche
  {
    public event PropertyChangedEventHandler PropertyChanged;
    private string matches = string.Empty;

    public CustomMatchesModel() { }

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
