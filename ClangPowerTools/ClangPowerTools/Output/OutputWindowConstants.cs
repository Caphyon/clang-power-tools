using System.Collections.Generic;

namespace ClangPowerTools
{
  public static class OutputWindowConstants
  {
    #region Constants

    public const string kPaneName = "Clang Power Tools";
    public const string kStart = "Start";
    public const string kDone = "Done";
    public static readonly Dictionary<int, string> kCommandsNames = new Dictionary<int, string>
    {
      {CommandIds.kCompileId, "Clang Compile"},
      {CommandIds.kCompileToolbarId, "Clang Compile"},
      {CommandIds.kTidyId, "Clang Tidy"},
      {CommandIds.TidyToolbarId, "Clang Tidy"},
      {CommandIds.kTidyFixToolbarId, "Clang Tidy-Fix"},
      {CommandIds.kTidyFixId, "Clang Tidy-Fix"}
    };

    #endregion
  }
}
