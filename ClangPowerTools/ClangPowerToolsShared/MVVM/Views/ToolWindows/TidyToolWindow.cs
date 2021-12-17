
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ClangPowerToolsShared.MVVM.Views.ToolWindows
{
  [Guid(WindowGuidString)]
  public class TidyToolWindow : ToolWindowPane
  {
    #region Members

    public const string WindowGuidString = "e4e2ba26-a455-4c53-adb3-8225fb696f9b";
    public const string Title = "Clang Power Tools - Tidy";
    private object tidyToolWindowView;
    private Type mObjType;

    #endregion

    #region Constructors

    public TidyToolWindow() : base()
    {
      Caption = Title;
      BitmapImageMoniker = KnownMonikers.ImageIcon;
      var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(
        asm => asm.GetName().FullName.Contains("ClangPowerToolsLib"));

      mObjType = assembly.GetType("ClangPowerTools.Views.TidyToolWindowView");
      tidyToolWindowView = Activator.CreateInstance(mObjType);

      Content = tidyToolWindowView;
    }

    #endregion

    public void UpdateToolWindow(List<string> filesPath)
    {
      MethodInfo method = mObjType.GetMethod("UpdateView");
      method.Invoke(tidyToolWindowView, new object[] { filesPath });
    }

    public void OpenTidyToolWindow(List<string> filesPath)
    {
      MethodInfo method = mObjType.GetMethod("OpenTidyToolWindow");
      method.Invoke(tidyToolWindowView, new object[] { filesPath });
    }
  }
}
