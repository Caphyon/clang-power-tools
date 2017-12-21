using System.Collections;

namespace ClangPowerTools
{
  public class UseChecksFromConvertor : ComboBoxConvertor
  {
    public UseChecksFromConvertor()
    {
      mValues = new ArrayList(new string[]
      {
        ComboBoxConstants.kPredefinedChecks,
        ComboBoxConstants.kCustomChecks,
        ComboBoxConstants.kTidyFile
      });
    }

  }
}
