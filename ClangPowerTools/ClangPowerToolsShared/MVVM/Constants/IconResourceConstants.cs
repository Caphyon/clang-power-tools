using System.IO;

namespace ClangPowerToolsShared.MVVM.Constants
{
  public static class IconResourceConstants
  {
    private static DirectoryInfo directoryPath = new DirectoryInfo(Directory.GetCurrentDirectory());
    public static string RefreshTidyDark { get; } = directoryPath.Parent.Parent.FullName + @"\Resources\TidyToolWindow\[CPT]Refresh_dark.png";
    public static string RefreshTidyLight { get; } = directoryPath.Parent.Parent.FullName + @"\Resources\TidyToolWindow\[CPT]Refresh-Tidy.png";
    public static string RefreshDisabled { get; } = directoryPath.Parent.Parent.FullName + @"\Resources\TidyToolWindow\[CPT]Refresh_disabled.png";

    public static string DiffDark { get; } = directoryPath.Parent.Parent.FullName + @"\Resources\TidyToolWindow\[CPT]Diff_dark.png";
    public static string DiffLight { get; } = directoryPath.Parent.Parent.FullName + @"\Resources\TidyToolWindow\[CPT]Diff2.png";
    public static string DiffDisabled { get; } = directoryPath.Parent.Parent.FullName + @"\Resources\TidyToolWindow\[CPT]Diff_disabled.png";

    public static string FixDark { get; } = directoryPath.Parent.Parent.FullName + @"\Resources\TidyToolWindow\[CPT]Fix_dark.png";
    public static string FixLight { get; } = directoryPath.Parent.Parent.FullName + @"\Resources\TidyToolWindow\[CPT]Fix.png";
    public static string FixDisabled { get; } = directoryPath.Parent.Parent.FullName + @"\Resources\TidyToolWindow\[CPT]Fix_disabled.png";

    public static string RemoveDark { get; } = directoryPath.Parent.Parent.FullName + @"\Resources\TidyToolWindow\[CPT]Remove_dark.png";
    public static string RemoveLight { get; } = directoryPath.Parent.Parent.FullName + @"\Resources\TidyToolWindow\[CPT]Remove.png";
    public static string RemoveDisabled { get; } = directoryPath.Parent.Parent.FullName + @"\Resources\TidyToolWindow\[CPT]Remove_disabled.png";

    public static string RemoveFixDark { get; } = directoryPath.Parent.Parent.FullName + @"\Resources\TidyToolWindow\[CPT]RemoveFix_dark.png";
    public static string RemoveFixLight { get; } = directoryPath.Parent.Parent.FullName + @"\Resources\TidyToolWindow\[CPT]RemoveFix.png";
    public static string RemoveFixDisabled { get; } = directoryPath.Parent.Parent.FullName + @"\Resources\TidyToolWindow\[CPT]RemoveFix_disabled.png";
  }
}
