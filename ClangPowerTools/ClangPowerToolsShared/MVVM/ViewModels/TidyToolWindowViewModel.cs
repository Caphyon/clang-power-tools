using ClangPowerTools.Views;
using Microsoft.VisualStudio.Shell;

namespace ClangPowerToolsShared.MVVM.ViewModels
{
  public class TidyToolWindowViewModel : ToolWindowPane
  {
    #region Members
    
    private readonly TidyToolWindowView tidyToolWindowView;

    #endregion

    #region Constructors

    public TidyToolWindowViewModel(TidyToolWindowView tidyToolWindowView)
    {
      this.tidyToolWindowView = tidyToolWindowView;
      this.Caption = "Tidy Tool window ";
      this.Content = new TidyToolWindowView();
    }

    #endregion
  }
}
