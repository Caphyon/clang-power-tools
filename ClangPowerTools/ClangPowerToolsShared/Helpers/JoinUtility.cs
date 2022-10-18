using ClangPowerToolsShared.MVVM.Models.ToolWindowModels;
using EnvDTE;

namespace ClangPowerTools.Helpers
{
  public static class JoinUtility
  {
    public static string Join(string separator, params string[] text)
    {
      return string.Join(separator, text);
    }

    //Add matcher keyword if is needed
    public static string AddMatcherKeyword(string matcher)
    {
      if ((matcher.Length > 0 && matcher[0] == 'm'))
      {
        return matcher;
      }
      else
      {
        return "m " + matcher;
      }
    }

  }
}
