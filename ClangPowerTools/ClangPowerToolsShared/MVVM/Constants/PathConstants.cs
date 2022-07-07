using ClangPowerTools.Commands;
using System;
using System.IO;

namespace ClangPowerToolsShared.MVVM.Constants
{
  public static class PathConstants
  {
    public static string CacheRepositoryPath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ClangPowerTools", "CacheRepository");
    public static string LlvmLitePath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ClangPowerTools", "LLVM_Lite");
    public static string GetPathToFindCommands()
    {
      var jsonCompilationDatabasePath = JsonCompilationDatabaseCommand.Instance.JsonDBPath;
      var solutionNameDir = new FileInfo(jsonCompilationDatabasePath).Directory.FullName;
      return Path.Combine(solutionNameDir, "commands_find.txt");
    }
  }
}
