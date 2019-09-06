namespace ClangPowerTools
{
  public class SettingsProvider
  {
    public static CompilerSettingsViewModel CompilerSettingsViewModel { get; set; } = new CompilerSettingsViewModel();
    public static FormatSettingsViewModel FormatSettingsViewModel { get; set; } = new FormatSettingsViewModel();
    public static TidySettingsViewModel TidySettingsViewModel { get; set; } = new TidySettingsViewModel();
    public static TidyChecksViewModel TidyChecksViewModel { get; set; } = new TidyChecksViewModel();
    public static GeneralSettingsViewModel GeneralSettingsViewModel { get; set; } = new GeneralSettingsViewModel();

    public CompilerSettingsModel GetCompilerSettingsModel()
    {
      return CompilerSettingsViewModel.CompilerModel;
    }

    public void SetCompilerSettingsModel(CompilerSettingsModel compilerSettingsModel)
    {
      CompilerSettingsViewModel.CompilerModel = compilerSettingsModel;
    }
  }
}
