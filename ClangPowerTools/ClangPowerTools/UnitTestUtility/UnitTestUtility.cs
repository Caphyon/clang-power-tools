using ClangPowerTools.Services;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Threading.Tasks;

namespace ClangPowerTools.Tests
{
  public static class UnitTestUtility
  {
    public static string GetVsVersion()
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      var dte = (DTE)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
      return dte.Version;
    }

    public static async System.Threading.Tasks.Task LoadPackageAsync()
    {
      var guid = Guid.Parse(RunClangPowerToolsPackage.PackageGuidString);
      var shell = (IVsShell7)ServiceProvider.GlobalProvider.GetService(typeof(SVsShell));
      await shell.LoadPackageAsync(ref guid);
    }
  }
}
