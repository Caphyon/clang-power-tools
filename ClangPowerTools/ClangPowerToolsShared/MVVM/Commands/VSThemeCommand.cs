using ClangPowerToolsShared.MVVM.Constants;
using Microsoft.VisualStudio.Shell;
using System.Linq;

namespace ClangPowerToolsShared.MVVM.Commands
{
  public static class VSThemeCommand
  {
    private const string darkThemeVs2022 = "4294638330";
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

    /// <summary>
    /// Get corresponding icon to visual studio theme
    /// </summary>
    public static string GetDiscardFixIconEnabled()
    {
      if (VSThemeCommand.GetCurrentVsTheme() == VsThemes.Dark)
        return IconResourceConstants.DiscardFixDark;
      else
        return IconResourceConstants.DiscardFixLight;
    }

    /// <summary>
    /// Get corresponding icon to visual studio theme
    /// </summary>
    public static string GetTidyFixIconEnabled()
    {
      if (VSThemeCommand.GetCurrentVsTheme() == VsThemes.Dark)
        return IconResourceConstants.FixDark;
      else
        return IconResourceConstants.FixLight;
    }

    /// <summary>
    /// Get corresponding icon to visual studio theme
    /// </summary>
    public static string GetRefreshTidyIconEnabled()
    {
      if (VSThemeCommand.GetCurrentVsTheme() == VsThemes.Dark)
        return IconResourceConstants.RefreshTidyDark;
      else
        return IconResourceConstants.RefreshTidyLight;
    }

    /// <summary>
    /// Get corresponding icon to visual studio theme
    /// </summary>
    public static string GetIgnoreIconEnabled()
    {
      if (VSThemeCommand.GetCurrentVsTheme() == VsThemes.Dark)
        return IconResourceConstants.RemoveDark;
      else
        return IconResourceConstants.RemoveLight;
    }

    /// <summary>
    /// Get corresponding icon to visual studio theme
    /// </summary>
    public static string GetDiffIconEnabled()
    {
      if (VSThemeCommand.GetCurrentVsTheme() == VsThemes.Dark)
        return IconResourceConstants.DiffDark;
      else
        return IconResourceConstants.DiffLight;
    }
  }
}
