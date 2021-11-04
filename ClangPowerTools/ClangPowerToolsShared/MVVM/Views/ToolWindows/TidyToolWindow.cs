using ClangPowerTools.Views;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ClangPowerToolsShared.MVVM.Views.ToolWindows
{
  [Guid(WindowGuidString)]
  public class TidyToolWindow : ToolWindowPane
  {
    #region Members

    public const string WindowGuidString = "e4e2ba26-a455-4c53-adb3-8225fb696f9b";
    public const string Title = "Clang Power Tools - Tidy";
    private TidyToolWindowView tidyToolWindowView;

    #endregion

    #region Constructors

    public TidyToolWindow() : base()
    {
      Caption = Title;
      BitmapImageMoniker = KnownMonikers.ImageIcon;
      tidyToolWindowView = new TidyToolWindowView();
      Content = tidyToolWindowView;
    }

    #endregion

    public void UpdateToolWindow(List<string> filesPath)
    {
      tidyToolWindowView.UpdateView(filesPath);
    }

    public void OpenTidyToolWindow(List<string> filesPath)
    {
      tidyToolWindowView.OpenTidyToolWindow(filesPath);
    }
  }
}
