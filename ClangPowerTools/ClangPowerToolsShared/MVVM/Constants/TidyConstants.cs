using System;
using System.IO;

namespace ClangPowerTools
{
  public static class TidyConstants
  {

    public static string FlagsUri { get; } = @"https://clang.llvm.org/extra/clang-tidy/checks/";
    // This is the guid and id for the Tools.DiffFiles command
    public static string ToolsDiffFilesCmd { get; } = "{5D4C0442-C0A2-4BE8-9B4D-AB1C28450942}";
    public static int ToolsDiffFilesId { get; } = 256;
    public static string LongFilePrefix { get; } = @"\\?\";
    public static string SolutionTempGuid { get; } = Guid.NewGuid().ToString();
    public static string TempsFolderPath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ClangPowerTools", "Temp");
  }
}
