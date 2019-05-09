using System.Collections.Generic;

namespace ClangPowerTools
{
  public static class OutputWindowConstants
  {
    #region Constants

    public const string paneName = "Clang Power Tools";
    public const string start = "Start";
    public const string done = "Done";
    public static readonly Dictionary<int, string> commandName = new Dictionary<int, string>
    {
      {CommandIds.kCompileId, "Clang Compile"},
      {CommandIds.kTidyId, "Clang Tidy"},
      {CommandIds.kTidyFixId, "Clang Tidy-Fix"},
      {CommandIds.kClangFormat, "Clang Format"}
    };

    #endregion
  }
}
