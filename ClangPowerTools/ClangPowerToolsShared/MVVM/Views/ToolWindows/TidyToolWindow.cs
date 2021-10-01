using ClangPowerTools.Views;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Text;

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
