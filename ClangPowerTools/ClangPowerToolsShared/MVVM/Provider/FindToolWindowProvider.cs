using ClangPowerToolsShared.MVVM.Models.ToolWindowModels;
using ClangPowerToolsShared.MVVM.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ClangPowerToolsShared.MVVM.Provider
{
  public class FindToolWindowProvider
  {
    private static readonly FindToolWindowProvider instance = new FindToolWindowProvider();
    private static List<AutoCompleteHistoryViewModel> autoCompleteHistory { get; set; } = new List<AutoCompleteHistoryViewModel>();
    public static FindToolWindowProvider Instance = new FindToolWindowProvider();
    public static List<AutoCompleteHistoryViewModel> AutoCompleteHistory { get { return autoCompleteHistory; } }
    public const int maxHistoryCount = 7;
    public const int maxFavoritHistoryCount = 3;
    public const int countToDelete = 3;
    public static void AddAutoCompleteHistory(AutoCompleteHistoryViewModel matcher)
    {
      if (autoCompleteHistory is null)
        autoCompleteHistory = new();
      autoCompleteHistory.Insert(0, matcher);
    }

    public static void RemoveFromFullList()
    {
      if (autoCompleteHistory.Count >= maxHistoryCount)
      {
        autoCompleteHistory = autoCompleteHistory.OrderBy(u => u.RememberAsFavorit ? 0 : 1).ToList();
        autoCompleteHistory.RemoveRange(maxFavoritHistoryCount, countToDelete);
      }
    }

    public static bool UpdateFavoriteValue(AutoCompleteHistoryModel autoCompleteHistoryViewModel)
    {
      var historyModel = autoCompleteHistory.Find(a => a.Value == autoCompleteHistoryViewModel.Value);
      if (historyModel != null)
      {
        if (autoCompleteHistoryViewModel.RememberAsFavorit &&
          autoCompleteHistory.FindAll(a => a.RememberAsFavorit == true).Count >= maxFavoritHistoryCount)
        {
          DialogResult dialogResult = MessageBox.Show("You reached the limit(50 matchers) of favorite custom matchers, unpin from favorite to add new matcher",
                                 "Clang Power Tools", MessageBoxButtons.OK, MessageBoxIcon.Information);
          return false;
        }
        historyModel.RememberAsFavorit = autoCompleteHistoryViewModel.RememberAsFavorit;
        return true;
      }
      return false;
    }

    public static void UpdateAutoCompleteList(List<AutoCompleteHistoryViewModel> autoCompleteHistoryViewModels)
    {
      if (autoCompleteHistory is null || autoCompleteHistoryViewModels is null)
        autoCompleteHistory = new List<AutoCompleteHistoryViewModel>();
      autoCompleteHistory = autoCompleteHistoryViewModels.OrderBy(u => u.RememberAsFavorit ? 0 : 1).ToList();
    }

    static FindToolWindowProvider() { }
    private FindToolWindowProvider() { }
  }
}
