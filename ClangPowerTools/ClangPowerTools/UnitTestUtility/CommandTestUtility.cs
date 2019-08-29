using ClangPowerTools.Commands;

namespace ClangPowerTools.Tests
{
  public static class CommandTestUtility
  {
    public static CommandController CommandController { get; set; }

    public static string ScriptCommand { get; set; }

    public static CompileCommand Compile{ get; set; }

    public static TidyCommand Tidy { get; set; }

    public static FormatCommand Format { get; set; }

    public static IgnoreFormatCommand IgnoreFormat { get; set; }

    public static IgnoreCompileCommand IgnoreCompile { get; set; }

    public static StopCommand Stop { get; set; }

    public static SettingsCommand Settings { get; set; }
  }
}
