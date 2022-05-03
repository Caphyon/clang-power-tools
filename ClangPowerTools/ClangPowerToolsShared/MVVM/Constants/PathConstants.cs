using System;
using System.IO;

namespace ClangPowerToolsShared.MVVM.Constants
{
  public static class PathConstants
  {
    public static string CacheRepositoryPath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ClangPowerTools", "CacheRepository");
    public static string LlvmLitePath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ClangPowerTools", "LLVM_Lite");
  }
}
