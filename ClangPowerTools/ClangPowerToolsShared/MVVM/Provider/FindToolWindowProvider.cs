using ClangPowerToolsShared.MVVM.ViewModels;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ClangPowerToolsShared.MVVM.Provider
{
  public class FindToolWindowProvider
  {
    private static readonly FindToolWindowProvider instance = new FindToolWindowProvider();
    private static List<AutoCompleteHistoryViewModel> autoCompleteHistory { get; set; } = new List<AutoCompleteHistoryViewModel>();
    public static FindToolWindowProvider Instance = new FindToolWindowProvider();
    public static List<AutoCompleteHistoryViewModel> AutoCompleteHistory { get { return autoCompleteHistory; } }

    public static void AddAutoCompleteHistory(AutoCompleteHistoryViewModel matcher)
    {
      autoCompleteHistory.Insert(0,matcher);
    }
    public static void UpdateAutoCompleteList(List<AutoCompleteHistoryViewModel> autoCompleteHistoryViewModels)
    {
      if(autoCompleteHistory is  null)
        autoCompleteHistory = new List<AutoCompleteHistoryViewModel>();
      autoCompleteHistory = autoCompleteHistoryViewModels;
    }

    static FindToolWindowProvider() { }
    private FindToolWindowProvider() { }
  }
}
