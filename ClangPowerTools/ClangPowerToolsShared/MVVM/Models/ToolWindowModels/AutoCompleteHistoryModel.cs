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
    private string pinIconPath { get; set; }

    public string PinIconPath
    {
      get { return pinIconPath; }
      set
      {
        pinIconPath = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PinIconPath"));
      }
    }

    public AutoCompleteHistoryModel()
    {
      InitIcons();
    }

    public void Pin()
    {
      PinIconPath = VSThemeCommand.GetDiscardFixIconEnabled();
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
      pinIconPath = VSThemeCommand.GetIgnoreIconEnabled();
      PinIconPath = pinIconPath;
    }
  }

}
