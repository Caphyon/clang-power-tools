using System.Collections.Generic;

namespace ClangPowerTools.Commands
{
  public class CommandsName
  {
    public static readonly List<string> kCommands = new List<string>
    {
      "Debug.Start",
      "Build.Compile",
      //"File.SaveSelectedItems",
      "File.SaveAll",
      "Build.BuildSolution",
      "Build.RebuildSolution",
      "Build.BuildOnlyProject",
      "Build.RebuildOnlyProject"
    };
  }
}
