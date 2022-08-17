using ClangPowerToolsShared.MVVM.ViewModels;
using System.Windows.Forms;

namespace ClangPowerToolsShared.MVVM.Provider
{
  public class FindToolWindowProvider
  {
    private static readonly FindToolWindowProvider instance = new FindToolWindowProvider();
    public static AutoCompleteHistoryViewModel AutoCompleteHistoryViewModel { get; set; } = new AutoCompleteHistoryViewModel();

    public static FindToolWindowProvider Instance = new FindToolWindowProvider();

    static FindToolWindowProvider() { }
    private FindToolWindowProvider() { }
  }
}
