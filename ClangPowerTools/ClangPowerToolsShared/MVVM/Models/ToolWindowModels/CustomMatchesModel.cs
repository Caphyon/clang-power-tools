using ClangPowerTools;
using ClangPowerTools.Helpers;
using ClangPowerToolsShared.Commands;
using ClangPowerToolsShared.MVVM.Constants;
using ClangPowerToolsShared.MVVM.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;

namespace ClangPowerToolsShared.MVVM.Models.ToolWindowModels
{
  public class CustomMatchesModel : ComponentVisibility, IViewMatcher
  {
    public event PropertyChangedEventHandler PropertyChanged;
    private string matches = string.Empty;

    public CustomMatchesModel() { }
    public string Name { get; } = "Custom matches";

    public int Id { get; } = FindCommandIds.kCustomMatchesId;

    public string Matchers
    {
      get { return matches; }
      set
      {
        matches = value;
        PowerShellWrapper.InteractivCommands = JoinUtility.AddMatcherKeyword(value);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Matches"));
      }
    }

    public string Details { get; } = "Ex: match functionDecl(hasName(\"test\"))  // Matches call expressions with name test";

  }
}
