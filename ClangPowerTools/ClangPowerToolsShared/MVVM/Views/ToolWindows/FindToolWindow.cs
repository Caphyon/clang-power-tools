using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace ClangPowerToolsShared.MVVM.Views.ToolWindows
{
  [Guid(WindowGuidString)]
  public class FindToolWindow : ToolWindowPane
  {
    #region Members
    public const string WindowGuidString = "b6c99e5a-0649-4973-922a-a1a2a4057aeb";
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

    //public void OpenFindToolWindow(List<string> filesPath)
    //{
    //  MethodInfo method = mObjType.GetMethod("OpenFindToolWindow");
    //  method.Invoke(findToolWindowView, new object[] { filesPath });
    //}

    #endregion

  }
}
