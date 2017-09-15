using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerTools
{
  public class RegexConstants
  {
    #region Constants

    public const string kFindAllPaths = @"(([a-z]:|\\\\[a-z0-9_.$]+\\[a-z0-9_.$]+)?(\\?(?:[^\\/:*?""<>|\r\n]+\\)+)[^\\/:*?""<>|\r\n]+)";
    public const string kFindAllNumbers = "[0-9]+";

    #endregion

  }
}
