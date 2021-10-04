using ClangPowerTools.Views;
using Microsoft.VisualStudio.Shell;

namespace ClangPowerToolsShared.MVVM.Views.ToolWindows
{
  public class TidyToolWindow : ToolWindowPane
  {
    #region Constructors

    public TidyToolWindow() : base()
    {
      Caption = "Tidy Tool window ";
      Content = new TidyToolWindowView();
    }

    #endregion
  }
}
