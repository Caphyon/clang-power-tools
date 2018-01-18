namespace ClangPowerTools
{
  public class ScriptConstants
  {
    #region Constants

    #region Clang Compile/Tidy constants

    public const string kPowerShellPath = @"WindowsPowerShell\v1.0\powershell.exe";
    public const string kScriptBeginning = @"PowerShell.exe -ExecutionPolicy Bypass -NoProfile -Noninteractive -command '&";
    public const string kScriptName = "clang-build.ps1";


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

    public const string kClangFlags = "-clang-flags";
    public const string kIncludeDirectores = "-include-dirs";
    public const string kProjectsToIgnore = "-proj-ignore"; 
    public const string kFilesToIgnore = "-file-ignore"; 

    public const string kVsVersion = "-vs-ver";
    public const string kVsEdition = "-vs-sku";
    public const string kActiveConfiguration = "-active-config";

    public const string kHeaderFilter = "-header-filter";
    public const string kTidyFile = ".clang-tidy";

<<<<<<< HEAD
=======
    public const string kClangFormatStyle = "-format-style";
>>>>>>> automatically run clang format after clang tidy

    #endregion

    #region Clang Format constants

    public const string kClangFormat = "clang-format.exe";
    public const string kAssumeFilename = "-assume-filename";
    public const string kFallbackStyle = "-fallback-style";
    public const string kSortIncludes = "-sort-includes";
    public const string kStyle = "-style";

    #endregion

    #endregion

  }
}
