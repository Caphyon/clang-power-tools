using System.Collections;

namespace ClangPowerTools.Convertors
{
  public class StyleConvertor : ComboBoxConvertor
  {
    public StyleConvertor() :
      base(
        new ArrayList(new string[]
        {
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
