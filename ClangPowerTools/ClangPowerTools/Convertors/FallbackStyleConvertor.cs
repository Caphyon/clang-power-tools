using System.Collections;

namespace ClangPowerTools.Convertors
{
  public class FallbackStyleConvertor : ComboBoxConvertor
  {
    public FallbackStyleConvertor() :
      base(
        new ArrayList(new string[]
        {
          ComboBoxConstants.kNone,
          ComboBoxConstants.kFile,
          ComboBoxConstants.kChromium,
          ComboBoxConstants.kGoogle,
          ComboBoxConstants.kLLVM,
          ComboBoxConstants.kMozilla,
          ComboBoxConstants.kWebKit
        }))
    { }

  }
}
