using System.IO;

namespace ClangPowerToolsShared.MVVM.Constants
{
  public static class IconResourceConstants
  {
    private static DirectoryInfo directoryPath = new DirectoryInfo(Directory.GetCurrentDirectory());
    public static string PinDark { get; } = @"/ClangPowerTools;component/Resources/TidyToolWindow/[CTP]Pinned_dark.png";
    public static string PinLight { get; } = @"/ClangPowerTools;component/Resources/TidyToolWindow/[CPT]Pinned.png";

    public static string UnpinDark { get; } = @"/ClangPowerTools;component/Resources/TidyToolWindow/[CTP]UnPinned_dark.png";
    public static string UnpinLight { get; } = @"/ClangPowerTools;component/Resources/TidyToolWindow/[CPT]UnPinned.png";

    public static string RefreshTidyDark { get; } =  @"/ClangPowerTools;component/Resources/TidyToolWindow/[CPT]Refresh_dark.png";
    public static string RefreshTidyLight { get; } = @"/ClangPowerTools;component/Resources/TidyToolWindow/[CPT]Refresh-Tidy.png";
    public static string RefreshDisabled { get; } = @"/ClangPowerTools;component/Resources/TidyToolWindow/[CPT]Refresh_disabled.png";

    public static string DiffDark { get; } = @"/ClangPowerTools;component/Resources/TidyToolWindow/[CPT]Diff_dark.png";
    public static string DiffLight { get; } = @"/ClangPowerTools;component/Resources/TidyToolWindow/[CPT]Diff2.png";
    public static string DiffDisabled { get; } = @"/ClangPowerTools;component/Resources/TidyToolWindow/[CPT]Diff_disabled.png";

    public static string FixDark { get; } = @"/ClangPowerTools;component/Resources/TidyToolWindow/[CPT]Fix_dark.png";
    public static string FixLight { get; } = @"/ClangPowerTools;component/Resources/TidyToolWindow/[CPT]Fix.png";
    public static string FixDisabled { get; } = @"/ClangPowerTools;component/Resources/TidyToolWindow/[CPT]Fix_disabled.png";

    public static string RemoveDark { get; } = @"/ClangPowerTools;component/Resources/TidyToolWindow/[CPT]Remove_dark.png";
    public static string RemoveLight { get; } = @"/ClangPowerTools;component/Resources/TidyToolWindow/[CPT]Remove.png";
    public static string RemoveDisabled { get; } = @"/ClangPowerTools;component/Resources/TidyToolWindow/[CPT]Remove_disabled.png";

    public static string DiscardFixDark { get; } = @"/ClangPowerTools;component/Resources/TidyToolWindow/[CPT]RemoveFix_dark.png";
    public static string DiscardFixLight { get; } = @"/ClangPowerTools;component/Resources/TidyToolWindow/[CPT]RemoveFix.png";
    public static string DiscardFixDisabled { get; } = @"/ClangPowerTools;component/Resources/TidyToolWindow/[CPT]RemoveFix_disabled.png";
  }
}
