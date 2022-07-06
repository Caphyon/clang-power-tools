using System.Collections.Generic;
using System.Linq;

namespace ClangPowerToolsShared.Commands
{
  public static class FindCommandIds
  {
    public static List<KeyValuePair<int, string>> matchers = new List<KeyValuePair<int, string>>()
    {
        new KeyValuePair<int, string>(1,"Function called with default argument(s)"),
    };

    public static List<KeyValuePair<int, string>> Matchers { get { return matchers; } }

    public static List<string> CommandsName { get { return matchers.Select(a => a.Value).ToList(); }  }
    public static List<int> CommandIds { get { return matchers.Select(a => a.Key).ToList(); }  }


    public const int kDefaultArgsId = 1;
  }
}
