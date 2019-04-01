using EnvDTE;
using Microsoft;
using Microsoft.VisualStudio.Shell;

namespace ClangPowerTools.Tests
{
  public class UnitTestUtility
  {
    public static string GetVsVersion()
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      var dte = (DTE)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
      return dte.Version;
    }
  }
}
