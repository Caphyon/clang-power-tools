using System.Collections.Generic;

namespace ClangPowerTools.Commands
{
  public static class VsCommands
  {
    public static List<string> SaveCommands { get; }
      = new List<string>() { "File.SaveAll", "File.SaveSelectedItems", "File.Save" };

    public static string Compile { get; } = "Build.Compile";

  }
}
