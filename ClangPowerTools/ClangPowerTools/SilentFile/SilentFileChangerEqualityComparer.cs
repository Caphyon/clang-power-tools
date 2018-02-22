using ClangPowerTools.SilentFile;
using System.Collections.Generic;

namespace ClangPowerTools
{
  public class SilentFileChangerEqualityComparer : IEqualityComparer<SilentFileChanger>
  {
    #region Public methods

    public bool Equals(SilentFileChanger obj1, SilentFileChanger obj2)
    {
      if (obj2 == null && obj1 == null)
        return true;
      else if (obj1 == null | obj2 == null)
        return false;
      else if (obj1.DocumentFileName.ToLower() == obj2.DocumentFileName.ToLower())
        return true;
      else
        return false;
    }

    public int GetHashCode(SilentFileChanger obj)
    {
      return obj.GetHashCode();
    }

    #endregion

  }
}
