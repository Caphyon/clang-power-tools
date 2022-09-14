using ClangPowerToolsShared.MVVM.Commands;
using ClangPowerToolsShared.MVVM.Constants;
using ClangPowerToolsShared.MVVM.Provider;
using ClangPowerToolsShared.MVVM.ViewModels;
using System;
using System.ComponentModel;

namespace ClangPowerToolsShared.MVVM.Models.ToolWindowModels
{
  public class AutoCompleteHistoryModel : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Value { get; set; } = string.Empty;
    private string visibility = string.Empty;
    public string Visibility
    {
      get { return visibility; }
      set
      {
        visibility = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Visibility"));
      }
    }
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

    public AutoCompleteHistoryModel(bool isHistory = false)
    {
      UpdateVisibility(isHistory);
    }

    public void Pin()
    {
      if (!FindToolWindowProvider.CheckRememberFavoritIsMax(this) || rememberAsFavorit)
      {
        rememberAsFavorit = !rememberAsFavorit;
        RememberAsFavorit = rememberAsFavorit;
        FindToolWindowProvider.UpdateFavoriteValue(this, rememberAsFavorit);
      }
      FindToolWindowProvider.UpdateFavoriteValue(this, !rememberAsFavorit);
      FindToolWindowHandler findToolWindowHandler = new FindToolWindowHandler();
      findToolWindowHandler.SaveMatchersHistoryData();
    }

    public AutoCompleteHistoryModel(AutoCompleteHistoryViewModel autoCompleteHistoryViewModel, bool isHistory = true)
    {
      Id = autoCompleteHistoryViewModel.Id;
      rememberAsFavorit = autoCompleteHistoryViewModel.RememberAsFavorit;
      RememberAsFavorit = rememberAsFavorit;
      Value = autoCompleteHistoryViewModel.Value;
      UpdateVisibility(isHistory);
      rememberAsFavorit = autoCompleteHistoryViewModel.RememberAsFavorit;
    }

    private void UpdateVisibility(bool isHistory)
    {
      if (isHistory)
        visibility = UIElementsConstants.Visibile;
      else
        visibility = UIElementsConstants.Hidden;
      Visibility = visibility;
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
