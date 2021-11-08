using System.Collections.Generic;

namespace ClangPowerTools
{
  public class ScriptConstants
  {
    #region Constants

    public const string kCppProjectGuid = "{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}";

    #region Clang Compile/Tidy constants

    public static readonly List<string> kAcceptedFileExtensions = new List<string>
      { ".c",
        ".cpp",
        ".cc",
        ".cxx",
        ".c++",
        ".cp",
        ".h",
        ".hh",
        ".hpp",
        ".hxx",
        ".tli",
        ".tlh",
        ".vcxproj"
      };

    public static readonly List<string> kExtendedAcceptedFileExtensions = new List<string>
      { ".c",
        ".cpp",
        ".cc",
        ".cxx",
        ".c++",
        ".cp",
        ".cs",
        ".h",
        ".hh",
        ".hpp",
        ".hxx",
        ".tli",
        ".tlh",
        ".vcxproj",
        ".inl"
      };

    public const string kProjectFileExtension = ".vcxproj";
    public const string FileExtensionsSelectFile = "Code files (*.c;*.cpp;*.cxx;*.cc;*.tli;*.tlh;*.h;*.hh;*.hpp;*.hxx;)|*.c;*.cpp;*.cxx;*.cc;*.tli;*.tlh;*.h;*.hh;*.hpp;*.hxx";

    public const string kCMakeConfigFile = "cmakelists.txt";

    public const string kPowerShellPath = @"WindowsPowerShell\v1.0\powershell.exe";
    public const string kScriptBeginning = @"PowerShell.exe -ExecutionPolicy Unrestricted -NoProfile -Noninteractive -command '&";
    public const string kScriptName = "clang-build.ps1";
    public const string ToolingV1 = @"Tooling\\v1";
    public const string kEnvrionmentTidyPath = "CLANG_TIDY_PATH";


    public const string kFile = "-file";
    public const string kProject = "-proj";
    public const string kDirectory = "-dir";
    public const string kLiteral = "-literal";

    public const string kContinue = "-continue";
    public const string kTreatWarningsAsErrors = "-Werror";
    public const string kParallel = "-parallel";
    public const string kVerboseMode = "-Verbose";
    public const string kSystemIncludeDirectories = "-treat-sai";

    public const string kTidy = "-tidy";
    public const string kTidyFix = "-tidy-fix";
    public const string kTidyCheckFirstElement = "-*,";

    public const string kClangFlags = "-clang-flags";
    public const string kIncludeDirectores = "-include-dirs";
    public const string kProjectsToIgnore = "-proj-ignore";
    public const string kFilesToIgnore = "-file-ignore";

    public const string kVsVersion = "-vs-ver";
    public const string kVsEdition = "-vs-sku";
    public const string kActiveConfiguration = "-active-config";

    public const string kHeaderFilter = "-header-filter";
    public const string kTidyFile = ".clang-tidy";

    public const string kClangFormatStyle = "-format-style";

    #endregion

    #region Clang Format constants

    public const string kClangFormat = "clang-format.exe";
    public const string kAssumeFilename = "-assume-filename";
    public const string kFallbackStyle = "-fallback-style";

    //public const string kSortIncludes = "-sort-includes";

    public const string kStyle = "-style";

    #endregion


    #region JSON Compilation DB

    public const string kJsonCompilationDb = "-export-jsondb";

    #endregion

    #endregion

  }
}
