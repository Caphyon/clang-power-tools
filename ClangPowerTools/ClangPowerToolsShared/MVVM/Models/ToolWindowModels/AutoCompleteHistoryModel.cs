using ClangPowerToolsShared.MVVM.Commands;
using ClangPowerToolsShared.MVVM.Provider;
using ClangPowerToolsShared.MVVM.ViewModels;
using System.ComponentModel;

namespace ClangPowerToolsShared.MVVM.Models.ToolWindowModels
{
  public class AutoCompleteHistoryModel : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    public string Value { get; set; } = string.Empty;
    private bool rememberAsFavorit = false;
    public bool RememberAsFavorit 
    { 
      get { return rememberAsFavorit; } 
      set
      {
        SetIcon(value);
      }
    }

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

    public AutoCompleteHistoryModel() { }

    public void Pin()
    {
      rememberAsFavorit = !rememberAsFavorit;
      RememberAsFavorit = rememberAsFavorit;
      FindToolWindowProvider.UpdateFavoriteValue(this);
      FindToolWindowHandler findToolWindowHandler = new FindToolWindowHandler();
      findToolWindowHandler.SaveMatchersHistoryData();
    }
    public AutoCompleteHistoryModel(AutoCompleteHistoryViewModel autoCompleteHistoryViewModel)
    {
      RememberAsFavorit = autoCompleteHistoryViewModel.RememberAsFavorit;
      Value = autoCompleteHistoryViewModel.Value;
    }

    private void SetIcon(bool value)
    {
      if (value)
        pinIconPath = VSThemeCommand.GetPinIcon();
      else
        pinIconPath = VSThemeCommand.GetUnpinIcon();
      PinIconPath = pinIconPath;
    }

  }

}
