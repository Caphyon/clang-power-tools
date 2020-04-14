using ClangPowerTools.MVVM.Views;
using ClangPowerTools.Views;

namespace ClangPowerTools
{
  public class SettingsProvider
  {
    #region Properties 

    public static CompilerSettingsModel CompilerSettingsModel { get; set; } = new CompilerSettingsModel();
    public static FormatSettingsModel FormatSettingsModel { get; set; } = new FormatSettingsModel();
    public static TidySettingsModel TidySettingsModel { get; set; } = new TidySettingsModel();
    public static GeneralSettingsModel GeneralSettingsModel { get; set; } = new GeneralSettingsModel();
    public static LlvmSettingsModel LlvmSettingsModel { get; set; } = new LlvmSettingsModel();
    public static SettingsView SettingsView { get; set; }
    public static FormatEditorView FormatEditorView { get; set; }

    public SettingsProvider Instance
    {
      get
      {
        return instance;
      }
    }

    #endregion

    #region Members

    private static readonly SettingsProvider instance = new SettingsProvider();

    #endregion

    #region Constructor

    // Explicit static constructor to tell C# compiler
    // not to mark type as beforefieldinit
    static SettingsProvider()
    {
    }

    private SettingsProvider()
    {
    }

    #endregion

  }
}
