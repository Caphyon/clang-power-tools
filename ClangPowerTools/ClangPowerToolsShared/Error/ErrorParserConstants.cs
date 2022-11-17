namespace ClangPowerTools
{
  public class ErrorParserConstants
  {
    #region Constants

    public const string kClangTag = "Clang Power Tools : ";
    public const string kCptMatcher = "🔎 Match : ";
    public const string kCompileClangMissingFromPath = "'clang++.exe' is not recognized";
    public const string kTidyClangMissingFromPath = "'clang-tidy.exe' is not recognized";
    public const string kErrorTag = "error";
    public const string kWarningTag = "warning";
    public const string kMessageTag = "message";
    public const string kErrorMessageRegex = @"(.\:(\\|\/)[ \S+\\\/.]*[c|C|h|H|cpp|CPP|cc|CC|cxx|CXX|c++|C++|cp|CP])(\r\n|\r|\n| |:)*(\d+)(\r\n|\r|\n| |:)*(\d+)(\r\n|\r|\n| |:)*(error|note|warning)[^s](\r\n|\r|\n| |:)*(.*)";
    public const string kMatchMessageRegex = @"<([A-Z]:.+?\.(cpp|cu|cc|cp|tlh|c|cxx|tli|h|hh|hpp|hxx)):(\d+):(\d+),\s(?:col:\d+|line:\d+:\d+)>";
    public const string kMatchTidyFileRegex = @"([A-Z]:\\.[^""]+?\.(cpp|cu|cc|cp|tlh|c|cxx|tli|h|hh|hpp|hxx))(\W|$)";
    public const string kNumberMatchesRegex = @"(\d+)\s(matches|match)\.";
    public const string kJsonCompilationDbFilePathRegex = @"Exported JSON Compilation Database to (.+\.json)";

    #endregion
  }
}
