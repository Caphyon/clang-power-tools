﻿using System.Collections.Generic;

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
      {CommandIds.kCompileToolbarId, "Clang Compile"},
      {CommandIds.kTidyId, "Clang Tidy"},
      {CommandIds.kTidyToolbarId, "Clang Tidy"},
      {CommandIds.kTidyFixId, "Clang Tidy-Fix"},
      {CommandIds.kTidyFixToolbarId, "Clang Tidy-Fix"},
      {CommandIds.kClangFormat, "Clang Format"},
      {CommandIds.kClangFormatToolbarId, "Clang Format"},
      {CommandIds.kJsonCompilationDatabase, "JSON Compilation Database"}
    };

    #endregion
  }
}
