using ClangPowerToolsShared.MVVM.Constants;
using Microsoft.VisualStudio.Shell;
using System.Linq;

namespace ClangPowerToolsShared.MVVM.Commands
{
  public static class VSThemeCommand
  {
    private const string darkThemeVs2022 = "4293256677";
    private const string darkThemeVs2019 = "4294046193";
    private const string textColorKey = "ToolWindowTextColorKey";

    public static VsThemes GetCurrentVsTheme()
    {
      var colorValues = VsColors.GetCurrentThemedColorValues();
      var cvPair = colorValues.Where(a => a.Key.ToString() == textColorKey).FirstOrDefault();
      if ((cvPair.Value.ToString() == darkThemeVs2022) || (cvPair.Value.ToString() == darkThemeVs2019))
        return VsThemes.Dark;
      return VsThemes.Light;
    }
  }
}
