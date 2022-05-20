using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClangPowerToolsShared.MVVM.Views.ToolWindows
{
  public class FindToolWindow : ToolWindowPane
  {
    #region Members
    public const string WindowGuidString = "CC497953-EA7E-431B-AF21-029C70E90C2D";
    public const string Title = "Clang Power Tools - Find";
    private object findToolWindowView;
    private Type mObjType;

    #endregion

    #region Constructors

    public FindToolWindow() : base()
    {
      Caption = Title;
      BitmapImageMoniker = KnownMonikers.ImageIcon;
      var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(
        asm => asm.GetName().FullName.Contains("ClangPowerToolsLib"));

      mObjType = assembly.GetType("ClangPowerTools.Views.FindToolWindowView");
      findToolWindowView = Activator.CreateInstance(mObjType);

      Content = findToolWindowView;
    }

    #endregion

  }
}
