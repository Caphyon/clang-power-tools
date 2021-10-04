using ClangPowerTools.Views;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using System.Runtime.InteropServices;

namespace ClangPowerToolsShared.MVVM.Views.ToolWindows
{
  [Guid(WindowGuidString)]
  public class TidyToolWindow : ToolWindowPane
  {
    #region Members

    public const string WindowGuidString = "7A1C87AD-8AFA-424E-9DCB-951BFFF050DC";
    public const string Title = "Sample Tool Window";

    #endregion

    #region Constructors

    public TidyToolWindow() : base()
    {
      Caption = "Tidy Tool window ";
      BitmapImageMoniker = KnownMonikers.ImageIcon;
      Content = new TidyToolWindowView();
    }

    #endregion
  }
}
