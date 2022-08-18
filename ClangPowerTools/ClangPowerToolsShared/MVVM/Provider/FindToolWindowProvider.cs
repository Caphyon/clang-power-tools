using ClangPowerToolsShared.MVVM.ViewModels;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ClangPowerToolsShared.MVVM.Provider
{
  public class FindToolWindowProvider
  {
    private static readonly FindToolWindowProvider instance = new FindToolWindowProvider();
    public static List<AutoCompleteHistoryViewModel> AutoCompleteHistory { get; set; } = new List<AutoCompleteHistoryViewModel>();
    public static FindToolWindowProvider Instance = new FindToolWindowProvider();

    static FindToolWindowProvider() { }
    private FindToolWindowProvider() { }
  }
}
