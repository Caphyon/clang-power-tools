using ClangPowerTools.Views;
using ClangPowerToolsShared.MVVM.ViewModels;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;

namespace ClangPowerToolsShared.MVVM.Views.ToolWindows
{
  [Guid(WindowGuidString)]
  public class TidyToolWindow : ToolWindowPane
  {
    #region Members

    public const string WindowGuidString = "e4e2ba26-a455-4c53-adb3-8225fb696f9b";
    public const string Title = "Sample Tool Window";
    private TidyToolWindowView tidyToolWindowView;

    #endregion

    #region Constructors

    public TidyToolWindow() : base()
    {
      Caption = "Tidy Tool window ";
      BitmapImageMoniker = KnownMonikers.ImageIcon;
      tidyToolWindowView = new TidyToolWindowView();
      Content = tidyToolWindowView;
    }

    #endregion
    public void UpdateToolWindow()
    {
      tidyToolWindowView.UpdateView();
    }
  }
}
