using System.Collections;

namespace ClangPowerTools
{
  public class HeaderFiltersConvertor : ComboBoxConvertor
  {
    public HeaderFiltersConvertor()
    {
      mValues = new ArrayList(new string[]
      {
        ComboBoxConstants.kDefaultHeaderFilter,
        ComboBoxConstants.kCorrespondingHeader
      });
    }

  }
}
