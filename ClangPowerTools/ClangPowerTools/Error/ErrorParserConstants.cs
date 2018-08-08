namespace ClangPowerTools
{
  public class ErrorParserConstants
  {
    #region Constants

    public const string kClangTag                     = "Clang : ";
    public const string kCompileClangMissingFromPath  = "'clang++.exe' is not recognized";
    public const string kTidyClangMissingFromPath     = "'clang-tidy.exe' is not recognized";
    public const string kMissingLlvmMessage           = "\n\nDid you forget to set-up LLVM?\n\nPlease follow these steps:\n- Go to http://releases.llvm.org/download.html. \n- Download the latest LLVM pre-build binaries for Windows. \n- Run installer. \n- During install, tick checkbox 'Add LLVM to the system PATH for all users/current user'. \n- Restart Visual Studio.";
    public const string kErrorTag                     = "error";
    public const string kWarningTag                   = "warning";
    public const string kMessageTag                   = "message";
    public const string kErrorMessageRegex = @"(.\:(\\|\/)[ \S+\\\/.]*[c|C|h|H|cpp|CPP|cc|CC|cxx|CXX|c++|C++|cp|CP])(\r\n|\r|\n| |:)*(\d+)(\r\n|\r|\n| |:)*(\d+)(\r\n|\r|\n| |:)*(error|note|warning)[^s](\r\n|\r|\n| |:)*(.*)";

    #endregion
  }
}
