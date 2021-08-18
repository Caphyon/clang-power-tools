using System.Collections.Generic;

namespace ClangPowerTools.MVVM.Constants
{
  public static class PsUpdaterConstants
  {
    public const string PowerShellScriptsFolder = "PowerShellScripts";
    public const string ToolingFolder = "Tooling";
    public const string V1Folder = "v1";
    public const string PsClangFolder = "psClang";
    public const string ClangBuildScript = "clang-build.ps1";
    public const string GitHubUri = @"https://raw.githubusercontent.com/Caphyon/clang-power-tools/master/ClangPowerTools/ClangPowerTools/Tooling/v1/";

    public static List<string> ScriptsInPsClangFolder { get; } = new()
    {
      "get-header-references.ps1",
      "io.ps1",
      "itemdefinition-context.ps1",
      "jsondb-export.ps1",
      "msbuild-expression-eval.ps1",
      "msbuild-project-data.ps1",
      "msbuild-project-load.ps1",
      "visualstudio-detection.ps1"
    };
  }
}
