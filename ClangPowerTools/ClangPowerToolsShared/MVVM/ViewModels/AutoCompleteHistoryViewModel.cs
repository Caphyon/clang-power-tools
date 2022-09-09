using System;

namespace ClangPowerToolsShared.MVVM.ViewModels
{
  public class AutoCompleteHistoryViewModel
  {
    public string Id { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool RememberAsFavorit { get; set; } = false;
  }
}
