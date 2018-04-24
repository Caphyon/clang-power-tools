using System;
using System.Windows.Interop;

namespace ClangPowerTools.Options
{
  // Enable the editing for all the UI elements
  public class UIElementsActivator
  {
    #region Members 

    private const UInt32 DLGC_WANTARROWS = 0x0001;
    private const UInt32 DLGC_WANTTAB = 0x0002;
    private const UInt32 DLGC_WANTALLKEYS = 0x0004;
    private const UInt32 DLGC_HASSETSEL = 0x0008;
    private const UInt32 DLGC_WANTCHARS = 0x0080;
    private const UInt32 WM_GETDLGCODE = 0x0087;

    #endregion

    #region Methods 

    public static void Activate(HwndSource aHwndSource)
    {
      if (aHwndSource != null)
        aHwndSource.AddHook(new HwndSourceHook(ChildHwndSourceHook));
    }

    private static IntPtr ChildHwndSourceHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
      if (msg == WM_GETDLGCODE)
      {
        handled = true;
        return new IntPtr(DLGC_WANTCHARS | DLGC_WANTARROWS | DLGC_HASSETSEL);
      }
      return IntPtr.Zero;
    }

    #endregion
  }
}
