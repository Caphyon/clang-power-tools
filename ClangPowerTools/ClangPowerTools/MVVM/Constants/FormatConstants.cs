namespace ClangPowerTools
{
  public static class FormatConstants
  {
    public const string FileExtensionsCodeFiles = "Code files (*.c;*.cpp;*.cxx;*.cc;*.tli;*.tlh;*.h;*.hh;*.hpp;*.hxx;)|*.c;*.cpp;*.cxx;*.cc;*.tli;*.tlh;*.h;*.hh;*.hpp;*.hxx";
    public const string BoldFontWeight = "Bold";
    public const string NormalFontWeight = "Normal";
    public const string DiffPerfectMatchFound = "// Clang Power Tools has found a perfect diff match for this file.\r\n//\r\n// Generate your Clang-Format file and start using it in your projects.";

    public const string DetectingTitle = "Detecting Clang-Format Style";
    public const string DetectingDescription = "Process will take some time depending on size (10 000 lines ~2 minutes)";
    public const string DetectingDescriptionExtra = "Larger code samples may result in detecting more format flags";

    public const string UpdateTitle = "Updating Format Preview";
    public const string UpdateDescription = "The changed Format Options are being applied to the Format Preview diff";

    public const string ResetTitle = "Restoring Format Options";
    public const string ResetDescription = "Restore to detected Format Options";
  }
}
