using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
