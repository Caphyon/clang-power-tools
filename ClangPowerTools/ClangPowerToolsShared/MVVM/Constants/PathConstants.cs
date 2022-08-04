using ClangPowerTools;
using ClangPowerTools.Services;
using EnvDTE;
using EnvDTE80;
using System;
using System.IO;

namespace ClangPowerToolsShared.MVVM.Constants
{
  public static class PathConstants
  {
    public static string CacheRepositoryPath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ClangPowerTools", "CacheRepository");
    public static string LlvmLitePath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ClangPowerTools", "LLVM_Lite");

    public static string SolutionDirPath
    {
      get
      {
        var dte2 = (DTE2)VsServiceProvider.GetService(typeof(DTE2));
        var solution = dte2.Solution.FullName;
        return new FileInfo(solution).Directory.FullName;
      }
    }

    private static string vcxprojPath = string.Empty;
    public static string VcxprojPath
    {
      get
      {
        if (vcxprojPath == string.Empty)
        {
          ItemsCollector itemsCollector = new();
          var items = itemsCollector.CollectActiveProjectItem();
          if (items is not null && items[0] is not null)
          {
            var projectItem = items[0].GetObject() as ProjectItem;
            vcxprojPath = projectItem.ContainingProject.FullName;

          }
        }
        return vcxprojPath;
      }
    }

    public static string GetPathToFindCommands
    {
      get
      {
        return Path.Combine(SolutionDirPath, "commands_find.txt");
      }
    }

    public static string JsonCompilationDBPath
    {
      get
      {
        return Path.Combine(SolutionDirPath, "compile_commands.json");
      }
    }

  }
}
