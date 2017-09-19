
namespace ClangPowerTools
{
  public class ScriptConstants
  {
    #region Constants

    public const string kScriptBeginning = @"PowerShell.exe -ExecutionPolicy Bypass -NoProfile -Noninteractive -command '&";
    public const string kScriptName = "clang-build.ps1";

    public const string kFile = "-file";
    public const string kProject = "-proj";
    public const string kDirectory = "-dir";

    public const string kContinue = "-continue";
    public const string kParallel = "-parallel";

    public const string kTidy = "-tidy";
    public const string kTidyFix = "-tidy-fix";

    public const string kClangFlags = "-clang-flags";
    public const string kIncludeDirectores = "-include-dirs";
    public const string kProjectsToIgnore = "-proj-ignore";

    public const string kVsVersion = "-vs-ver";
    public const string kVsEdition = "-vs-sku";

    #endregion

  }
}
