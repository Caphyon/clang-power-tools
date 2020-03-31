using ClangPowerTools.MVVM.Views;
using ClangPowerTools.Views;

namespace ClangPowerTools
{
  public class SettingsProvider
  {
    private static CompilerSettingsModel compilerSettingsModel = new CompilerSettingsModel();
    private static FormatSettingsModel formatSettingsModel = new FormatSettingsModel();
    private static TidySettingsModel tidySettingsModel = new TidySettingsModel();
    private static GeneralSettingsModel generalSettingsModel = new GeneralSettingsModel();

    public static SettingsView SettingsView { get; set; }

    public static FormatEditorView FormatEditorView { get; set; }

    public CompilerSettingsModel GetCompilerSettingsModel()
    {
      return compilerSettingsModel;
    }

    public FormatSettingsModel GetFormatSettingsModel()
    {
      return formatSettingsModel;
    }

    public TidySettingsModel GetTidySettingsModel()
    {
      return tidySettingsModel;
    }

    public GeneralSettingsModel GetGeneralSettingsModel()
    {
      return generalSettingsModel;
    }

    public void SetCompilerSettingsModel(CompilerSettingsModel model)
    {
      compilerSettingsModel = model;
    }

    public void SetFormatSettingsModel(FormatSettingsModel model)
    {
      formatSettingsModel = model;
    }

    public void SetTidySettingsModel(TidySettingsModel model)
    {
      tidySettingsModel = model;
    }

    public void SetGeneralSettingsModel(GeneralSettingsModel model)
    {
      generalSettingsModel = model;
    }
  }
}
