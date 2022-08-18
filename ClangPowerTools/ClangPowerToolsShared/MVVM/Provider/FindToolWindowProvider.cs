using ClangPowerToolsShared.MVVM.ViewModels;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ClangPowerToolsShared.MVVM.Provider
{
  public class FindToolWindowProvider
  {
    private static readonly FindToolWindowProvider instance = new FindToolWindowProvider();
    public static List<string> AutoCompleteHistory { get; set; } = new List<string>();
    public static FindToolWindowProvider Instance = new FindToolWindowProvider();

    static FindToolWindowProvider() { }
    private FindToolWindowProvider() { }
  }
}
