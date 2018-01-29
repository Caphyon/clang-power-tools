using System.Collections;

namespace ClangPowerTools.Convertors
{
  class AdditionalIncludesConvertor : ComboBoxConvertor
  {
    public AdditionalIncludesConvertor() :
     base(
       new ArrayList(new string[]
       {
          ComboBoxConstants.kIncludeDirectories,
          ComboBoxConstants.kSystemIncludeDirectories
       }))
    { }

  }
}
