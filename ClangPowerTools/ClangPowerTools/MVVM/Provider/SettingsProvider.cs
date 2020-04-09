using ClangPowerTools.MVVM.Views;
using ClangPowerTools.Views;

namespace ClangPowerTools
{
  public class SettingsProvider
  {
    public static CompilerSettingsModel CompilerSettingsModel { get; set; } = new CompilerSettingsModel();
    public static FormatSettingsModel FormatSettingsModel { get; set; } = new FormatSettingsModel();
    private static TidySettingsModel tidySettingsModel = new TidySettingsModel();
    private static GeneralSettingsModel generalSettingsModel = new GeneralSettingsModel();
    private static LlvmSettingsModel llvmSettingsModel = new LlvmSettingsModel();
    private static LlvmModel preinstalledLlvm = new LlvmModel();

    public static SettingsView SettingsView { get; set; }

    public static FormatEditorView FormatEditorView { get; set; }


    public TidySettingsModel GetTidySettingsModel()
    {
      return tidySettingsModel;
    }

    public GeneralSettingsModel GetGeneralSettingsModel()
    {
      return generalSettingsModel;
    }

    public LlvmSettingsModel GetLlvmSettingsModel()
    {
      return llvmSettingsModel;
    }

    public LlvmModel GetPreinstalledLLvmModel()
    {
      return preinstalledLlvm;
    }

    public void SetTidySettingsModel(TidySettingsModel model)
    {
      tidySettingsModel = model;
    }

    public void SetGeneralSettingsModel(GeneralSettingsModel model)
    {
      generalSettingsModel = model;
    }

    public void SetLlvmSettingsModel(LlvmSettingsModel model)
    {
      llvmSettingsModel = model;
    }

    public void SetPreinstalledLLvmModel(LlvmModel model)
    {
       preinstalledLlvm = model;
    }
  }
}
