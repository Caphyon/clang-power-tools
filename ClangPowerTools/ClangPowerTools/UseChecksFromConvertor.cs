using System.Collections;

namespace ClangPowerTools
{
  public class UseChecksFromConvertor : ComboBoxConvertor
  {
    public UseChecksFromConvertor() :
      base(
        new ArrayList(new string[]
      {
        ComboBoxConstants.kPredefinedChecks,
        ComboBoxConstants.kCustomChecks,
        ComboBoxConstants.kTidyFile
      }))
    { }

  }
}
