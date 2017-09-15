
namespace ClangPowerTools
{
  public class ScriptConstants
  {
    #region Constants

    public const int kFaild = 47;
    public const int kSuccess = 0;

    public const string kScriptBeginning = @"PowerShell.exe -ExecutionPolicy Bypass -NoProfile -Noninteractive -command '&";
    public const string kScriptName = "clang-build.ps1";

    public const string kCompileCommand = "Compile";
    public const string kTidyCommand = "Tidy";
    public const string kParallel = "-parallel";

    #endregion

  }
}
