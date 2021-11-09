using System;
using System.IO;

namespace ClangPowerToolsShared.MVVM.Constants
{
  public static class PathConstants
  {
    public static string CPTPath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ClangPowerTools");
    public static string CacheRepositoryPath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ClangPowerTools", "CacheRepository");
  }
}
