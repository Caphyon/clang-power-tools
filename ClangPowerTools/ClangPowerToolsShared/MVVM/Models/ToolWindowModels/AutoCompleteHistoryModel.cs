using ClangPowerToolsShared.MVVM.Commands;
using ClangPowerToolsShared.MVVM.Constants;
using ClangPowerToolsShared.MVVM.ViewModels;
using System.ComponentModel;

namespace ClangPowerToolsShared.MVVM.Models.ToolWindowModels
{
  public class AutoCompleteHistoryModel : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    public string Value { get; set; } = string.Empty;
    public bool RememberAsFavorit { get; set; } = false;
    private IconModel pinIcon { get; set; }

    public IconModel PinIcon
    {
      get { return pinIcon; }
      set
      {
        pinIcon = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PinIcon"));
      }
    }

    public AutoCompleteHistoryModel()
    {
      InitIcons();
    }

    public AutoCompleteHistoryModel(string value, bool remembaerAsFavorit = false)
    {
      InitIcons();
      Value = value;
      RememberAsFavorit = remembaerAsFavorit;
    }

    public AutoCompleteHistoryModel(AutoCompleteHistoryViewModel autoCompleteHistoryViewModel)
    {
      InitIcons();
      RememberAsFavorit = autoCompleteHistoryViewModel.RememberAsFavorit;
      Value = autoCompleteHistoryViewModel.Value;
    }

    private void InitIcons()
    {
      pinIcon = new IconModel(VSThemeCommand.GetDiffIconEnabled(), UIElementsConstants.Visibile, true);
      PinIcon = new IconModel(VSThemeCommand.GetDiffIconEnabled(), UIElementsConstants.Visibile, true);
    }
  }

}
