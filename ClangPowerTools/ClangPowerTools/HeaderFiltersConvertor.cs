using System.Collections;

namespace ClangPowerTools
{
  public class HeaderFiltersConvertor : ComboBoxConvertor
  {
    public HeaderFiltersConvertor() : 
      base(
        new ArrayList(new string[]
        {
          ComboBoxConstants.kDefaultHeaderFilter,
          ComboBoxConstants.kCorrespondingHeader
        }))
    {}

  }
}
