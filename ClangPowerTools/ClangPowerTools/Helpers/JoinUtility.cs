using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerTools.Helpers
{
  public static class JoinUtility
  {
    public static string Join(string separator, params string[] text)
    {
      return string.Join(separator, text);
    }

  }
}
